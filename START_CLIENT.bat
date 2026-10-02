@echo off
cd /d "%~dp0"
dotnet run --project Content.Client -c Release --no-build
pause
