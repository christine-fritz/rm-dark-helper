@echo off
setlocal
cd /d "%~dp0"

set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

if not exist "%CSC%" (
    echo.
    echo ERROR: 64-bit .NET Framework compiler not found:
    echo %CSC%
    echo.
    echo Enable/install ".NET Framework 4.8 Advanced Services".
    echo.
    pause
    exit /b 1
)

echo Building RM Dark Helper...
"%CSC%" ^
  /nologo ^
  /target:winexe ^
  /platform:x64 ^
  /optimize+ ^
  /out:"RMDarkHelper.exe" ^
  /reference:System.dll ^
  /reference:System.Core.dll ^
  /reference:Microsoft.CSharp.dll ^
  /reference:System.Drawing.dll ^
  /reference:System.Windows.Forms.dll ^
  src\*.cs

if errorlevel 1 (
    echo.
    echo BUILD FAILED.
    pause
    exit /b 1
)

echo.
echo DONE: %CD%\RMDarkHelper.exe
echo.
pause
