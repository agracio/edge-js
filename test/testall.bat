@echo off

set SELF=%~dp0
set run32=N
set run64=N

if "%1"=="" set run32=Y
if "%1"=="ia32" set run32=Y
if "%1"=="" set run64=Y
if "%1"=="x64" set run64=Y

if "%run32%"=="Y" (
	call "%SELF%\test.bat" ia32 19.3.0
	call "%SELF%\test.bat" ia32 18.4.0
	call "%SELF%\test.bat" ia32 16.15.1
	call "%SELF%\test.bat" ia32 14.19.3
)

if "%run64%"=="Y" (
	call "%SELF%\test.bat" x64 19.3.0
	call "%SELF%\test.bat" x64 18.4.0
	call "%SELF%\test.bat" x64 16.15.1
	call "%SELF%\test.bat" x64 14.19.3
)