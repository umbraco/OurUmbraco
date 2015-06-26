@ECHO OFF
SET release=%1
ECHO Installing Npm NuGet Package

SET nuGetFolder=%CD%\..\packages\
ECHO Configured packages folder: %nuGetFolder%
ECHO Current folder: %CD%

%CD%\..\.nuget\NuGet.exe install Npm.js -OutputDirectory %nuGetFolder%  -Verbosity quiet

for /f "delims=" %%A in ('dir %nuGetFolder%node.js.* /b') do set "nodePath=%nuGetFolder%%%A\"
for /f "delims=" %%A in ('dir %nuGetFolder%npm.js.* /b') do set "npmPath=%nuGetFolder%%%A\tools\"

ECHO Temporarily adding Npm and Node to path
SET oldPath=%PATH%

path=%npmPath%;%nodePath%;%PATH%

ECHO %path%

SET buildFolder=%CD%

ECHO Change directory to %CD%\..\OurUmbraco.Client\
CD %CD%\..\OurUmbraco.Client\

ECHO Do npm install and the gulp build
call npm install
call npm install -g install gulp -g --quiet
call gulp

ECHO Reset path to what it was before
path=%oldPath%

ECHO Move back to the build folder
CD %buildFolder% 