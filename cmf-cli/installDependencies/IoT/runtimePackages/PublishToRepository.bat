@echo off

set NPM=http://127.0.0.1:4873
set TAG="IoT_805"
set FORCE=

rem Uncomment the next line if you plan to deploy over a deployed version
rem set FORCE="--force"

echo Deploying %TAG% to %NPM%"...


call npm adduser --registry %NPM%

forfiles /m *.tgz /c "cmd /c NPM publish @path --registry %NPM% --tag %TAG% %FORCE%"

timeout 10