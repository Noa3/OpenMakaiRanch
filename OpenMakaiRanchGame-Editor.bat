@echo off
setlocal

set "PROJECT_PATH=%~dp0OpenMakaiRanchGame"
set "LOCAL_GODOT=%~dp0Godot_v4.6.3-stable_mono_win64.exe"
set "LOCAL_GODOT_CONSOLE=%~dp0Godot_v4.6.3-stable_mono_win64_console.exe"

if exist "%LOCAL_GODOT%" (
  start "OpenMakaiRanchGame" "%LOCAL_GODOT%" --path "%PROJECT_PATH%"
  exit /b 0
)

if exist "%LOCAL_GODOT_CONSOLE%" (
  start "OpenMakaiRanchGame" "%LOCAL_GODOT_CONSOLE%" --path "%PROJECT_PATH%"
  exit /b 0
)

where godot >nul 2>nul
if %errorlevel%==0 (
  start "OpenMakaiRanchGame" godot --path "%PROJECT_PATH%"
  exit /b 0
)

where godot_mono >nul 2>nul
if %errorlevel%==0 (
  start "OpenMakaiRanchGame" godot_mono --path "%PROJECT_PATH%"
  exit /b 0
)

where Godot_mono >nul 2>nul
if %errorlevel%==0 (
  start "OpenMakaiRanchGame" Godot_mono --path "%PROJECT_PATH%"
  exit /b 0
)

echo Could not find Godot editor executable.
echo 1. Place Godot_v4.6.3-stable_mono_win64.exe in this repo root, or
echo 2. Place Godot_v4.6.3-stable_mono_win64_console.exe in this repo root, or
echo 3. Install Godot and ensure godot/godot_mono is on PATH.
exit /b 1
