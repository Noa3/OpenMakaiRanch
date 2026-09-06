@echo off
setlocal
cd /d "%~dp0"
dotnet build "OpenMakaiRanchGame\OpenMakaiRanchGame.csproj"
if errorlevel 1 exit /b 1
python -m unittest discover -s Tools\Godot -p "test_*.py"
if errorlevel 1 exit /b 1
python "Tools\Godot\launch.py" --mode smoke %*
exit /b %errorlevel%
