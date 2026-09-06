@echo off
setlocal
python "%~dp0Tools\Godot\launch.py" --mode editor %*
exit /b %errorlevel%
