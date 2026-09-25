@echo off
setlocal EnableExtensions

if "%GTA5_DIR%"=="" (
  echo GTA5_DIR is not set.
  echo Example:
  echo   set GTA5_DIR=C:\Program Files (x86)\Steam\steamapps\common\Grand Theft Auto V
  exit /b 1
)

if not exist "%~dp0bin\Release\SPCore.dll" (
  echo SPCore.dll is missing. Run build.bat first.
  exit /b 1
)

mkdir "%GTA5_DIR%\scripts" 2>nul
copy /Y "%~dp0bin\Release\SPCore.dll" "%GTA5_DIR%\scripts\SPCore.dll" >nul
copy /Y "%~dp0config\inventory_config.json" "%GTA5_DIR%\scripts\inventory_config.json" >nul

if exist "%~dp0inventory_icons" (
  mkdir "%GTA5_DIR%\scripts\inventory_icons" 2>nul
  xcopy /E /I /Y "%~dp0inventory_icons" "%GTA5_DIR%\scripts\inventory_icons" >nul
)

echo.
echo SPCore installed to:
echo   %GTA5_DIR%\scripts\
echo.
exit /b 0
