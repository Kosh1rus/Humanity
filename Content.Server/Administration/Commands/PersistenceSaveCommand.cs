using Content.Shared.Administration;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Console;
using Robust.Shared.Map;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Utility;
using Content.Server.Humanity.Persistence;

namespace Content.Server.Administration.Commands;

[AdminCommand(AdminFlags.Server)]
public sealed partial class PersistenceSave : LocalizedEntityCommands
{
    [Dependency] private IConfigurationManager _config = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private MapLoaderSystem _mapLoader = default!;
    [Dependency] private NomadWorldSaveSystem _worldSave = default!;

    public override string Command => "persistencesave";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 1 || args.Length > 2)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
            return;
        }

        if (!int.TryParse(args[0], out var intMapId))
        {
            shell.WriteError(Loc.GetString("cmd-parse-failure-integer", ("arg", args[0])));
            return;
        }

        var mapId = new MapId(intMapId);
        if (!_map.MapExists(mapId))
        {
            shell.WriteError(Loc.GetString("cmd-savemap-not-exist"));
            return;
        }

        var saveFilePath = (args.Length > 1 ? args[1] : null) ?? _config.GetCVar(CCVars.GameMap);
        if (string.IsNullOrWhiteSpace(saveFilePath))
        {
            shell.WriteError(Loc.GetString("cmd-persistencesave-no-path", ("cvar", nameof(CCVars.GameMap))));
            return;
        }

        var success = args.Length == 1 && _config.GetCVar(CCVars.UsePersistence)
            ? _worldSave.Save(_map.GetMap(mapId))
            : _mapLoader.TrySaveMap(mapId, new ResPath(saveFilePath));
        if (!success)
        {
            shell.WriteError("Не удалось сохранить мир. Проверьте серверный журнал.");
            return;
        }
        shell.WriteLine(Loc.GetString("cmd-savemap-success"));
    }
}
