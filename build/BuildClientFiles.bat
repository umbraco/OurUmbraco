@ECHO OFF
SETLOCAL
	:: SETLOCAL is on, so changes to the path not persist to the actual user's path
	
SET release=%1
SET toolsFolder=%CD%\tools\
ECHO Current folder: %CD%

SET nodeFileName=node-v4.5.0-win-x86.7z
SET nodeExtractFolder=%toolsFolder%node.js.450
IF NOT EXIST %nodeExtractFolder% (
	ECHO Downloading node.js
	powershell -Command "(New-Object Net.WebClient).DownloadFile('http://nodejs.org/dist/v4.5.0/%nodeFileName%', '%toolsFolder%\%nodeFileName%')"
	ECHO Extracting node.js
	%toolsFolder%\7za.exe x %toolsFolder%\%nodeFileName% -o%nodeExtractFolder% -aos > nul
)
FOR /f "delims=" %%A in ('dir %nodeExtractFolder%\node* /b') DO SET "nodePath=%nodeExtractFolder%\%%A"

SET nuGetFolder=%CD%\..\packages\
FOR /f "delims=" %%A in ('dir %nuGetFolder%npm.js.* /b') DO SET "npmPath=%nuGetFolder%%%A\tools\"
IF [%npmPath%] == [] GOTO :installnpm 
IF NOT [%npmPath%] == [] GOTO :build

:installnpm
	ECHO Downloading npm
	ECHO Configured packages folder: %nuGetFolder%	
	ECHO Installing Npm NuGet Package
	%CD%\..\.nuget\NuGet.exe install Npm.js -OutputDirectory %nuGetFolder%  -Verbosity quiet
	REM Ensures that we look for the just downloaded NPM, not whatever the user has installed on their machine
	FOR /f "delims=" %%A in ('dir %nuGetFolder%npm.js.* /b') DO SET "npmPath=%nuGetFolder%%%A\tools\"
	GOTO :build

:build
	PATH=%npmPath%;%nodePath%
	SET buildFolder=%CD%

	ECHO Change directory to %CD%\..\OurUmbraco.Client\
	CD %CD%\..\OurUmbraco.Client\

	ECHO Do npm install and the gulp build
	SET npm="%nodePath%\node.exe" "%npmPath%node_modules\npm\bin\npm-cli.js" %*
	%npm% cache clean
	%npm% install
	%npm% install -g install gulp -g
	gulp build

	ECHO Move back to the build folder
	CD %buildFolder% 
	GOTO :EOF