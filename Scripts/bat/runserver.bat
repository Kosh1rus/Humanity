@echo off
cd..
cd..
dotnet run --project Content.Server -c Release -- --config-file Resources/ConfigPresets/Civ/production.toml
pause
