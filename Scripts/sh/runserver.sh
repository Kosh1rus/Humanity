#!/bin/sh
dotnet run --project Content.Server -c Release -- --config-file Resources/ConfigPresets/Civ/production.toml
read -p "Press enter to continue"
