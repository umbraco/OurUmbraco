@ECHO OFF
SETLOCAL
	
SET release=%1
SET toolsFolder=%CD%\tools\
ECHO Current folder: %CD%

SET nodeFileName=node-v8.9.4-win-x86.7z
SET nodeExtractFolder="%toolsFolder%node.js.894"
IF NOT EXIST %nodeExtractFolder% (
	ECHO Downloading node.js
	powershell -Command "(New-Object Net.WebClient).DownloadFile('http://nodejs.org/dist/v8.9.4/%nodeFileName%', '%toolsFolder%\%nodeFileName%')"
	ECHO Extracting node.js
	%toolsFolder%\7za.exe x %toolsFolder%\%nodeFileName% -o%nodeExtractFolder% -aos > nul
)
FOR /f "delims=" %%A in ('dir %nodeExtractFolder%\node* /b') DO SET "nodePath=%nodeExtractFolder%\%%A"

:build
	SET npmPath=%nodePath%\node_modules\npm\bin
	
	PATH=%npmPath%;%nodePath%
	SET buildFolder=%CD%

	ECHO Change directory to %CD%\..\OurUmbraco.Client\
	CD %CD%\..\OurUmbraco.Client\

	ECHO.
	ECHO Setting node_modules folder to hidden to prevent VS13 from crashing on it while loading the websites project
	attrib +h node_modules
	
	ECHO Adding Npm and Node to path 
	:: SETLOCAL is on, so changes to the path not persist to the actual user's path
	PATH="%nodePath%";"%npmPath%"	

	SET npm="%nodePath%\node.exe" "%npmPath%\npm-cli.js" %*

	ECHO NPM install dependencies
	%npm% install
	%npm% install --save-dev jshint gulp-jshint
	%npm% install -g install gulp -g
	%npm% rebuild node-sass
	
	ECHO Gulp build the client side files
	gulp build

	ECHO Move back to the build folder
	CD %buildFolder% 
	GOTO :EOF