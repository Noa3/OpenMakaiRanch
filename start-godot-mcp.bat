@echo off
setlocal enabledelayedexpansion

:: OpenMakaiRanch - Start Godot Editor + MCP Server

echo [1/3] Checking Node.js...
where node >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Node.js not found. Install Node.js 20+ first.
    pause
    exit /b 1
)
for /f "tokens=*" %%i in ('node --version') do set NODE_VER=%%i
echo   Node.js: !NODE_VER!

echo [2/3] Starting Godot Editor...
set GODOT_EXE=E:\OpenMakaiRanch\Godot_v4.7-stable_mono_win64.exe
if not exist "!GODOT_EXE!" (
    echo ERROR: Godot exe not found at !GODOT_EXE!
    pause
    exit /b 1
)

:: Launch Godot in EDITOR mode (not game) by passing a scene file
start "Godot Editor" "!GODOT_EXE!" --editor --path "E:\OpenMakaiRanch\OpenMakaiRanchGame" "E:\OpenMakaiRanch\OpenMakaiRanchGame\scenes\Bootstrap.tscn"
echo   Godot Editor started.

echo [3/3] Starting MCP Server...
cd /d "C:\Users\noa3\Documents\GodotMCP"
start "Godot MCP Server" cmd /k "npx godot-universal-mcp"

echo.
echo ============================================
echo  Godot Editor + MCP Server started!
echo  Editor bridge: 127.0.0.1:9500
echo  Runtime bridge: 127.0.0.1:9501
echo  MCP Server: stdio (npx godot-universal-mcp)
echo ============================================
echo.
echo  Clients can now connect:
echo    - Hermes: mcp_servers.godotMCP
echo    - VS Code: .vscode/mcp.json
echo    - Open Code: opencode.json
echo.
echo  To stop: close the terminal windows.
echo.
pause
