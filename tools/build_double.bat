@echo off
set SELF=%~dp0

if "%1" equ "" (
    echo Usage: build_double.bat {node_version}
    echo e.g. build_double.bat 20.12.2
    exit /b -1
)

call :build_lib
if %ERRORLEVEL% neq 0 exit /b -1

call :build_tools
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

call :clean_nuget_package
if %ERRORLEVEL% neq 0 exit /b -1
call :copy_nuget_package
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
@REM if not exist "%SELF%\build\unzip.exe" (
@REM 	csc /out:"%SELF%\build\unzip.exe" "%SELF%\unzip.cs"
@REM ) else (
@REM      echo "%SELF%\build\unzip.exe" already built.
@REM )

exit /b 0

REM ===========================================================
:download_node
echo :download_node %1

if not exist "%SELF%\build\%1.zip" (
    node "%SELF%\download_double.js" http://github.com/nodejs/node/archive/v%1.zip "%SELF%\build\%1.zip"
) else (
    echo "%SELF%\build\%1.zip" already exists.
)

echo :unzip %1.zip
if not exist "%SELF%\build\node-%1" (
	rem "%SELF%\build\unzip.exe" "%SELF%\build\%1.zip" "%SELF%\build"
	pushd "%SELF%\build\"
    powershell Expand-Archive -Path %1.zip -DestinationPath "%SELF%\build\"
	rem cscript //B ..\unzip.vbs %1.zip
	popd
) else (
     echo "%SELF%\build\node-%1" already exists.
)

rem git clone --depth 1 --branch 20.12.2 https://github.com/nodejs/node.git node-20.12.2



exit /b 0

REM ===========================================================
:build_lib
echo :build_lib

if exist "%SELF%\build\nuget\lib\net462" (
 echo "%SELF%\build\nuget\lib\net462" already exists.
 exit /b 0
 )

mkdir "%SELF%\..\src\double\Edge.js\bin\Release\net462" > nul 2>&1

csc /out:"%SELF%\..\src\double\Edge.js\bin\Release\net462\EdgeJs.dll" /target:library "%SELF%\..\src\double\Edge.js\dotnet\EdgeJs.cs"
if %ERRORLEVEL% neq 0 exit /b -1

cd "%SELF%\..\src\double\Edge.js"
dotnet restore

if %ERRORLEVEL% neq 0 exit /b -1
dotnet build --configuration Release

if %ERRORLEVEL% neq 0 exit /b -1
mkdir "%SELF%\build\nuget\lib"
robocopy /NFL /NDL /NJH /NJS /nc /ns /np /is /s "%SELF%\..\src\double\Edge.js\bin\Release" "%SELF%\build\nuget\lib"
rem robocopy /NFL /NDL /NJH /NJS /nc /ns /np /is /s "%SELF%\..\src\double\Edge.js\bin\Release\net45" "%SELF%\build\nuget\lib\net45"
rem robocopy /NFL /NDL /NJH /NJS /nc /ns /np /is /s "%SELF%\..\src\double\Edge.js\bin\Release\net6.0" "%SELF%\build\nuget\lib\net6.0"

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
rem rmdir /s /q build
rmdir /s /q tools\icu\Release

call vcbuild.bat release %2 dll
if not exist .\Release\libnode.dll (
    echo Cannot build libnode.dll for %1-%2
    popd
    exit /b -1
)

mkdir "%SELF%\build\node-%1-%2"
copy /y .\Release\node.lib "%SELF%\build\node-%1-%2"
copy /y .\Release\libnode.dll "%SELF%\build\node-%1-%2"
copy /y .\Release\libnode.lib "%SELF%\build\node-%1-%2"
echo Finished building Node shared library %1

popd
exit /b 0

REM ===========================================================
:download_node_exe
echo :download_node_exe

if not exist "%SELF%\build\node-%1-x86\node.exe" (
    echo Downloading Node.js binary to  "%SELF%\build\node-%1-x86\node.exe"
    node "%SELF%\download_double.js" http://nodejs.org/dist/v%1/win-x86/node.exe "%SELF%\build\node-%1-x86\node.exe"
) else (
    echo "%SELF%\build\node-%1-x86\node.exe" already exists.
)

if not exist "%SELF%\build\node-%1-x64\node.exe" (
    echo Downloading Node.js binary to "%SELF%\build\node-%1-x64\node.exe"
    node "%SELF%\download_double.js" http://nodejs.org/dist/v%1/win-x64/node.exe "%SELF%\build\node-%1-x64\node.exe"
) else (
    echo "%SELF%\build\node-%1-x64\node.exe" already exists.
)

exit /b 0

REM ===========================================================
:build_edge
echo :build_edge %1 %2 %3

rem takes 3 parameters: 1 - node version, 2 - x86 or x64, 3 - ia32 or x64

if exist "%SELF%\build\nuget\content\edge\%2\edge_nativeclr.node" (
 echo "%SELF%\build\nuget\content\edge\%2\edge_nativeclr.node" already built.
 exit /b 0
)
FOR /F "tokens=* USEBACKQ" %%F IN (`npm config get prefix`) DO (SET NODEBASE=%%F)

set NODEEXE=%SELF%\build\node-%1-%2\node.exe

copy /Y %SELF%\build\node-%1-%2\libnode.lib %LOCALAPPDATA%\node-gyp\Cache\%1\%3

set GYP=%NODEBASE%\node_modules\node-gyp\bin\node-gyp.js

pushd "%SELF%\.."

"%NODEEXE%" "%GYP%" configure --msvs_version=2019
if %ERRORLEVEL% neq 0 (
    echo Error configuring edge.node %FLAVOR% for node.js %2 v%3
    exit /b -1
)

FOR %%F IN (build\*.vcxproj) DO (
    echo Patch node.lib in %%F
    powershell -Command "(Get-Content -Raw %%F) -replace '\\\\node.lib', '\\\\libnode.lib' | Out-File -Encoding Utf8 %%F"
)
"%NODEEXE%" "%GYP%" build
mkdir "%SELF%\build\nuget\content\edge\%2" > nul 2>&1
copy /y build\release\edge_nativeclr.node "%SELF%\build\nuget\content\edge\%2"
copy /y "%SELF%\build\node-%1-%2\libnode.dll" "%SELF%\build\nuget\content\edge\%2"

popd

exit /b 0

REM ===========================================================
:clean_nuget_package
echo :cleaning nuget publish folder

rmdir "nuget/content" /s /q
rmdir "nuget/lib" /s /q

exit /b 0

REM ===========================================================
:copy_nuget_package
echo :copying build to nuget publish folder

ROBOCOPY ../lib nuget/content/edge/ *.js /NFL /NDL /NJH /NJS /nc /ns /np
ROBOCOPY build/nuget/content/edge/x86 nuget/content/edge/x86 *.* /NFL /NDL /NJH /NJS /nc /ns /np
ROBOCOPY build/nuget/content/edge/x64 nuget/content/edge/x64 *.* /NFL /NDL /NJH /NJS /nc /ns /np

ROBOCOPY build/nuget/lib/net462 nuget/lib/net462 edge*.dll /NFL /NDL /NJH /NJS /nc /ns /np

rem nuget pack
exit /b 0
