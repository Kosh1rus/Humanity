@echo off
cd /d "%~dp0"
dotnet run --project Content.Server -c Release --no-build -- --config-file Resources/ConfigPresets/Civ/production.toml
pause
