using Content.Server.Administration;
using Content.Server.Humanity.Research;
using Content.Shared.Administration;
using Content.Shared.Humanity.Research;
using Robust.Shared.Console;
using Robust.Shared.Map;

namespace Content.Server.Humanity.Commands;

[AdminCommand(AdminFlags.Server)]
public sealed class SetEraCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entities = default!;
    [Dependency] private readonly IMapManager _maps = default!;

    public string Command => "setera";
    public string Description => "Меняет эпоху цивилизации на выбранной карте.";
    public string Help => "setera <эпоха 0–8> [mapId]\n" +
                          "Без mapId используется карта вашего персонажа. Из серверной консоли укажите mapId.\n" +
                          "0 — каменный век; 1 — бронзовый; 2 — железный; 3 — средневековье; " +
                          "4 — возрождение; 5 — индустриальная эпоха; 6 — мировые войны; " +
                          "7 — современность; 8 — новейшая эпоха.\n" +
                          "Прогресс внутри эпохи и лимит проведённых опытов сбрасываются.";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length is < 1 or > 2 || !int.TryParse(args[0], out var age) || age is < 0 or > 8)
        {
            shell.WriteError(Help);
            return;
        }

        MapId mapId;
        if (args.Length == 2)
        {
            if (!int.TryParse(args[1], out var id))
            {
                shell.WriteError("mapId должен быть целым числом.");
                return;
            }
            mapId = new MapId(id);
        }
        else if (_entities.TryGetComponent<TransformComponent>(shell.Player?.AttachedEntity, out var transform))
        {
            mapId = transform.MapID;
        }
        else
        {
            shell.WriteError("Укажите mapId: setera <эпоха 0–8> <mapId>.");
            return;
        }

        if (!_maps.MapExists(mapId))
        {
            shell.WriteError("Такой карты нет.");
            return;
        }

        if (!_entities.System<NomadResearchSystem>().TrySetAge(_maps.GetMapEntityId(mapId), age))
        {
            shell.WriteError("На этой карте эпоха недоступна: нет развития цивилизации, включён TDM или ограничен уровень исследований.");
            return;
        }

        shell.WriteLine($"Карта {mapId}: установлена эпоха «{NomadResearch.AgeName(age)}». Прогресс эпохи и счётчик опытов сброшены.");
    }
}
