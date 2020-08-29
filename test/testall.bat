@echo off

set SELF=%~dp0
set run32=N
set run64=N

if "%1"=="" set run32=Y
if "%1"=="ia32" set run32=Y
if "%1"=="" set run64=Y
if "%1"=="x64" set run64=Y

if "%run32%"=="Y" (
	call "%SELF%\test.bat" ia32 14.3.0
	call "%SELF%\test.bat" ia32 13.0.1
	call "%SELF%\test.bat" ia32 12.13.0
	call "%SELF%\test.bat" ia32 10.14.0
)

if "%run64%"=="Y" (
	call "%SELF%\test.bat" x64 14.3.0
	call "%SELF%\test.bat" x64 13.0.1
	call "%SELF%\test.bat" x64 12.13.0
	call "%SELF%\test.bat" x64 10.14.0
)