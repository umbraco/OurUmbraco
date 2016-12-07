@ECHO OFF
SETLOCAL
	:: SETLOCAL is on, so changes to the path not persist to the actual user's path

SET release=%1
SET buildFolder=%CD%

SET nuGetFolder=%CD%\..\packages
ECHO Configured packages folder: %nuGetFolder%
ECHO Current folder: %CD%

ECHO Installing Npm NuGet Package
"%CD%\..\.nuget\NuGet.exe" install Npm.js -OutputDirectory "%nuGetFolder%"  -Verbosity quiet

for /f "delims=" %%A in ('dir "%nuGetFolder%\node.js.*" /b') do set "nodePath=%nuGetFolder%\%%A\"
for /f "delims=" %%A in ('dir "%nuGetFolder%\npm.js.*" /b') do set "npmPath=%nuGetFolder%\%%A\tools\"

REM Ensures that we look for the just downloaded NPM, not whatever the user has installed on their machine
SET PATH=%npmPath%;%nodePath%;%PATH%
ECHO Path
ECHO %PATH%

ECHO Node
node -v

ECHO Change directory to "%CD%\..\OurUmbraco.Client"
CD "%CD%\..\OurUmbraco.Client"

ECHO.
ECHO Setting node_modules folder to hidden to prevent VS13 from crashing on it while loading the websites project
attrib +h node_modules

ECHO Npm Cache Clean
call npm cache clean
ECHO Npm Install
call npm install
ECHO Npm Install Gulp
call npm install -g install gulp -g --quiet
ECHO Gulp build
call gulp build

ECHO Move back to the build folder
CD %buildFolder% 