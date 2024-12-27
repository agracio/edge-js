@echo off
set SELF=%~dp0
if "%1" equ "" (
    echo Usage: build.bat debug^|release {version}
    echo e.g. build.bat release 20.12.2
    echo e.g. build.bat release 20
    exit /b -1
)
rmdir /S /Q ..\build\

for /F "delims=." %%a in ("%2") do set MAJORVERSION=%%a
set MAJORVERSION=%MAJORVERSION: =%

SET FLAVOR=%1
if "%FLAVOR%" equ "" set FLAVOR=release

shift

SET DESTDIRROOT=%SELF%\..\lib\native\win32

set VERSION=%1

call :node_version

pushd %SELF%\..

if %MAJORVERSION% LSS 23 (
    call :build ia32 x86 %VERSION%
    call :stamp_version
    call :copy
    if %ERRORLEVEL% neq 0 exit /b -1
)

call :build x64 x64 %VERSION%
call :stamp_version
call :copy
if %ERRORLEVEL% neq 0 exit /b -1

if %MAJORVERSION% GEQ 20 (
    call :build arm64 arm64 %VERSION%
    call :stamp_version
    call :copy
    if %ERRORLEVEL% neq 0 exit /b -1
)
    
popd

exit /b 0

@REM ===================================================================================
:node_version

if %MAJORVERSION% equ %VERSION% (
    echo Getting latest version of Node.js v%VERSION%
    FOR /F "tokens=* USEBACKQ" %%F IN (`node "%SELF%\getVersion.js" "%VERSION%"`) DO (SET VERSION=%%F)
) 

if %MAJORVERSION% equ %VERSION% (
    echo Cannot determine Node.js version for %VERSION%
    exit /b -1
)

exit /b 0

@REM ===================================================================================
:build

SET ARCH=%2
set DESTDIR=%DESTDIRROOT%\%1\%MAJORVERSION%
if not exist "%DESTDIR%" mkdir "%DESTDIR%"

echo Building edge.node %FLAVOR% for node.js %2 v%3

FOR /F "tokens=* USEBACKQ" %%F IN (`npm config get prefix`) DO (SET NODEBASE=%%F)
set GYP=%NODEBASE%\node_modules\node-gyp\bin\node-gyp.js
echo %GYP%

if not exist "%GYP%" (
    echo Cannot find node-gyp at %GYP%. Make sure to install with npm install node-gyp -g
    exit /b -1
)

node "%GYP%" configure --msvs_version=2022 -%FLAVOR% --target=%3 --runtime=node --arch=%2
if %ERRORLEVEL% neq 0 (
    echo Error configuring edge.node %FLAVOR% for node.js %2 v%3
    exit /b -1
)

@REM Conflict when building arm64 binaries
if "%2" == "arm64" (
    FOR %%F IN (build\*.vcxproj) DO (
        echo Patch /fp:strict in %%F
        powershell -Command "(Get-Content -Raw %%F) -replace '<FloatingPointModel>Strict</FloatingPointModel>', '<!-- <FloatingPointModel>Strict</FloatingPointModel> -->' | Out-File -Encoding Utf8 %%F"
    )
)

node "%GYP%" build

exit /b 0

@REM ===================================================================================
:stamp_version

type NUL > %DESTDIR%\node.version
echo %VERSION%> %DESTDIR%\node.version

exit /b 0

@REM ===================================================================================
:copy

echo %DESTDIR%
copy /y .\build\%FLAVOR%\edge_*.node "%DESTDIR%"
if %ERRORLEVEL% neq 0 (
    echo Error copying edge.node %FLAVOR% for node.js %ARCH% v%VERSION%
    exit /b -1
)
rmdir /S /Q .\build\
copy /y "%DESTDIR%\..\*.dll" "%DESTDIR%"
if %ERRORLEVEL% neq 0 (
    echo Error copying VC redist %FLAVOR% to %DESTDIR%
    exit /b -1
)

echo Success building edge.node %FLAVOR% for node.js %ARCH% v%VERSION%

exit /b 0
