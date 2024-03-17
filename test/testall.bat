@echo off

set SELF=%~dp0
set run32=N
set run64=N

if "%1"=="" set run32=Y
if "%1"=="ia32" set run32=Y
if "%1"=="" set run64=Y
if "%1"=="x64" set run64=Y

if "%run32%"=="Y" (
	call "%SELF%\test.bat" ia32 21.7.1
	call "%SELF%\test.bat" ia32 20.11.1
	call "%SELF%\test.bat" ia32 18.19.1
	call "%SELF%\test.bat" ia32 16.20.2
)

if "%run64%"=="Y" (
	call "%SELF%\test.bat" x64 21.7.1
	call "%SELF%\test.bat" x64 20.11.1
	call "%SELF%\test.bat" x64 18.19.1
	call "%SELF%\test.bat" x64 16.20.2
)