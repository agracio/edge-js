@echo off
set SELF=%~dp0
if "%1" equ "" (
    echo Usage: build.bat debug^|release "{version}"
    echo e.g. build.bat release "20.12.2"
    exit /b -1
)
FOR /F "tokens=* USEBACKQ" %%F IN (`node -p process.arch`) DO (SET ARCH=%%F)
for /F "delims=." %%a in ("%2") do set MAJORVERSION=%%a
set MAJORVERSION=%MAJORVERSION: =%

SET FLAVOR=%1
shift
if "%FLAVOR%" equ "" set FLAVOR=release
for %%i in (node.exe) do set NODEEXE=%%~$PATH:i
if not exist "%NODEEXE%" (
    echo Cannot find node.exe
    popd
    exit /b -1
)
for %%i in ("%NODEEXE%") do set NODEDIR=%%~dpi
SET DESTDIRROOT=%SELF%\..\lib\native\win32
set VERSION=%1
pushd %SELF%\..

if "%ARCH%" == "arm64" (
    call :build arm64 arm64 %VERSION%
) else (
    call :build ia32 x86 %VERSION%
    call :build x64 x64 %VERSION%
)
popd

exit /b 0

:build

set DESTDIR=%DESTDIRROOT%\%1\%MAJORVERSION%
if not exist "%DESTDIR%" mkdir "%DESTDIR%"
type NUL > %DESTDIR%\v%VERSION%

if exist "%DESTDIR%\node.exe" goto gyp
echo Downloading node.exe %2 %3...
node "%SELF%\download.js" %2 %3 "%DESTDIR%"
if %ERRORLEVEL% neq 0 (
    echo Cannot download node.exe %2 v%3
    exit /b -1
)

:gyp

echo Building edge.node %FLAVOR% for node.js %2 v%3
set NODEEXE=%DESTDIR%\node.exe
FOR /F "tokens=* USEBACKQ" %%F IN (`npm config get prefix`) DO (SET NODEBASE=%%F)
set GYP=%NODEBASE%\node_modules\node-gyp\bin\node-gyp.js
echo %GYP%
if not exist "%GYP%" (
    echo Cannot find node-gyp at %GYP%. Make sure to install with npm install node-gyp -g
    exit /b -1
)

"%NODEEXE%" "%GYP%" configure --msvs_version=2022 -%FLAVOR%
if %ERRORLEVEL% neq 0 (
    echo Error building edge.node %FLAVOR% for node.js %2 v%3
    exit /b -1
)

REM Conflict when building arm64 binaries
if "%ARCH%" == "arm64" (
    FOR %%F IN (build\*.vcxproj) DO (
    echo Patch /fp:strict in %%F
    powershell -Command "(Get-Content -Raw %%F) -replace '<FloatingPointModel>Strict</FloatingPointModel>', '<!-- <FloatingPointModel>Strict</FloatingPointModel> -->' | Out-File -Encoding Utf8 %%F"
    )
)

"%NODEEXE%" "%GYP%" build

echo %DESTDIR%
copy /y .\build\%FLAVOR%\edge_*.node "%DESTDIR%"
if %ERRORLEVEL% neq 0 (
    echo Error copying edge.node %FLAVOR% for node.js %2 v%3
    exit /b -1
)

copy /y "%DESTDIR%\..\*.dll" "%DESTDIR%"
if %ERRORLEVEL% neq 0 (
    echo Error copying VC redist %FLAVOR% to %DESTDIR%
    exit /b -1
)

echo Success building edge.node %FLAVOR% for node.js %2 v%3
