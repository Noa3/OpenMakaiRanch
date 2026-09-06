@echo off
setlocal
REM A stdio server must be spawned by its MCP client, not a detached terminal.
echo Editor bridge: 127.0.0.1:9500; runtime bridge: 127.0.0.1:9501
echo MCP client command: node "%~dp0Tools\Godot\bridge_server.mjs"
echo See Tools\Godot\README.md for dependencies and verification.
python "%~dp0Tools\Godot\launch.py" --mode editor %*
exit /b %errorlevel%
