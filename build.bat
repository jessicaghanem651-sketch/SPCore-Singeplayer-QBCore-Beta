@echo off
setlocal EnableExtensions

echo.
echo ================================================
echo   SPCore ^| Singleplayer QBCore Beta - Build Helper
echo ================================================
echo.

if "%GTA5_DIR%"=="" (
  echo GTA5_DIR is not set.
  echo.
  echo Set it to your GTA V installation folder, for example:
  echo   set GTA5_DIR=C:\Program Files (x86)\Steam\steamapps\common\Grand Theft Auto V
  echo.
  echo The folder must contain ScriptHookVDotNet3.dll.
  echo.
  exit /b 1
)

if not exist "%GTA5_DIR%\ScriptHookVDotNet3.dll" (
  echo ERROR: ScriptHookVDotNet3.dll was not found in:
  echo   %GTA5_DIR%
  echo.
  echo Install ScriptHookVDotNet 3 / use the SHVDN build required by your GTA V version.
  echo.
  exit /b 1
)

dotnet build "%~dp0SPCore.sln" -c Release /p:GTA5_DIR="%GTA5_DIR%"
if errorlevel 1 (
  echo.
  echo Build failed.
  exit /b 1
)

echo.
echo Build succeeded.
echo Output:
echo   %~dp0bin\Release\SPCore.dll
echo.
echo Use install.bat to copy the runtime files into GTA V.
echo.
exit /b 0
