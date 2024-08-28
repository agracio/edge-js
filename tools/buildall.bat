@echo off
FOR /F "tokens=* USEBACKQ" %%F IN (`node -p process.arch`) DO (SET ARCH=%%F)
if "%ARCH%" == "arm64" (
    "%~dp0\build.bat" release 20 22
) else (
    "%~dp0\build.bat" release 16 18 20 22
)
