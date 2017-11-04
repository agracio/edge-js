@echo off
set SELF=%~dp0

if "%1" equ "" (
    echo Usage: build_double.bat {node_version}
    echo e.g. build_double.bat 5.9.1
    exit /b -1
)

call :build_tools
if %ERRORLEVEL% neq 0 exit /b -1

call :build_lib
if %ERRORLEVEL% neq 0 exit /b -1

call :download_node %1
if %ERRORLEVEL% neq 0 exit /b -1

call :build_node %1 x86
if %ERRORLEVEL% neq 0 exit /b -1
call :build_node %1 x64
if %ERRORLEVEL% neq 0 exit /b -1

call :download_node_exe %1
if %ERRORLEVEL% neq 0 exit /b -1

call :build_edge %1 x86 ia32
if %ERRORLEVEL% neq 0 exit /b -1
call :build_edge %1 x64 x64
if %ERRORLEVEL% neq 0 exit /b -1

exit /b 0

REM ===========================================================
:build_tools
echo :build_tools

if not exist "%SELF%\build\download.exe" (
	csc /out:"%SELF%\build\download.exe" "%SELF%\download.cs"
) else (
    echo "%SELF%\build\download.exe" already built.
)

if not exist "%SELF%\build\repl.exe" (
	csc /out:"%SELF%\build\repl.exe" "%SELF%\repl.cs"
) else (
     echo "%SELF%\build\repl.exe" already built.
)

exit /b 0

REM ===========================================================
:download_node
echo :download_node %1

if not exist "%SELF%\build\%1.zip" (
	"%SELF%\build\download.exe" https://github.com/nodejs/node/archive/v%1.zip "%SELF%\build\%1.zip"
) else (
    echo "%SELF%\build\%1.zip" already exists.
)

if not exist "%SELF%\build\node-%1" (
	rem "%SELF%\build\unzip.exe" "%SELF%\build\%1.zip" "%SELF%\build"
	pushd "%SELF%\build\"
	cscript //B ..\unzip.vbs %1.zip
	popd
) else (
     echo "%SELF%\build\node-%1" already exists.
)


exit /b 0

REM ===========================================================
:build_lib
echo :build_lib

if exist "%SELF%\build\nuget\lib\net40" (
 echo "%SELF%\build\nuget\lib\net40" already exists.
 exit /b 0
 )

mkdir "%SELF%\..\src\double\Edge.js\bin\Release\net40" > nul 2>&1

csc /out:"%SELF%\..\src\double\Edge.js\bin\Release\net40\EdgeJs.dll" /target:library "%SELF%\..\src\double\Edge.js\dotnet\EdgeJs.cs"
if %ERRORLEVEL% neq 0 exit /b -1

cd "%SELF%\..\src\double\Edge.js"
dotnet restore

if %ERRORLEVEL% neq 0 exit /b -1
dotnet build --configuration Release

if %ERRORLEVEL% neq 0 exit /b -1
mkdir "%SELF%\build\nuget\lib"
robocopy /NFL /NDL /NJH /NJS /nc /ns /np /is /s "%SELF%\..\src\double\Edge.js\bin\x64\Release" "%SELF%\build\nuget\lib"
robocopy /NFL /NDL /NJH /NJS /nc /ns /np /is /s "%SELF%\..\src\double\Edge.js\bin\Release\net40" "%SELF%\build\nuget\lib\net40"

cd "%SELF%"
exit /b 0

REM ===========================================================
:build_node
echo :build_node %1 %2

if exist "%SELF%\build\node-%1-%2\node.lib" (
 echo "%SELF%\build\node-%1-%2\node.lib" already built
 exit /b 0
 )

pushd "%SELF%\build\node-%1"
rmdir /s /q Release
rmdir /s /q build
rmdir /s /q tools\icu\Release

call vcbuild.bat release %2 dll
if not exist .\Release\node.dll (
    echo Cannot build node.dll for %1-%2
    popd
    exit /b -1
)

mkdir "%SELF%\build\node-%1-%2"
copy /y .\Release\node.* "%SELF%\build\node-%1-%2"
echo Finished building Node shared library %1

popd
exit /b 0

REM ===========================================================
:download_node_exe
echo :download_node_exe

if not exist "%SELF%\build\node-%1-x86\node.exe" (
    echo Downloading Node.js binary to  "%SELF%\build\node-%1-x86\node.exe"
	"%SELF%\build\download.exe" http://nodejs.org/dist/v%1/win-x86/node.exe "%SELF%\build\node-%1-x86\node.exe"
) else (
    echo "%SELF%\build\node-%1-x86\node.exe" already exists.
)

if not exist "%SELF%\build\node-%1-x64\node.exe" (
    echo Downloading Node.js binary to "%SELF%\build\node-%1-x64\node.exe"
	"%SELF%\build\download.exe" http://nodejs.org/dist/v%1/win-x64/node.exe "%SELF%\build\node-%1-x64\node.exe"
) else (
    echo "%SELF%\build\node-%1-x64\node.exe" already exists.
)

exit /b 0

REM ===========================================================
:build_edge
echo :build_edge %1 %2 %3

rem takes 2 parameters: 1 - node version, 2 - x86 or x64

if exist "%SELF%\build\nuget\content\edge\%2\edge_nativeclr.node" (
 echo "%SELF%\build\nuget\content\edge\%2\edge_nativeclr.node" already built.
 exit /b 0
)

set NODEEXE=%SELF%\build\node-%1-%2\node.exe
set GYP=%APPDATA%\npm\node_modules\node-gyp\bin\node-gyp.js

pushd "%SELF%\.."

"%NODEEXE%" "%GYP%" configure --msvs_version=2017
"%SELF%\build\repl.exe" ./build/edge_nativeclr.vcxproj "%USERPROFILE%\.node-gyp\%1\%3\node.lib" "%SELF%\build\node-%1-%2\node.lib"
"%NODEEXE%" "%GYP%" build
mkdir "%SELF%\build\nuget\content\edge\%2" > nul 2>&1
copy /y build\release\edge_nativeclr.node "%SELF%\build\nuget\content\edge\%2"
copy /y "%SELF%\build\node-%1-%2\node.dll" "%SELF%\build\nuget\content\edge\%2"

popd

exit /b 0
