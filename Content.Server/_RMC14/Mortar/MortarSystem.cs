using Content.Server.Popups;
using Content.Shared._RMC14.Mortar;
using Robust.Server.Containers;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using static Content.Shared.Popups.PopupType;

namespace Content.Server._RMC14.Mortar;

public sealed partial class MortarSystem : SharedMortarSystem
{
    [Dependency] private ContainerSystem _container = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    protected override bool CanLoadPopup(Entity<MortarComponent> mortar, Entity<MortarShellComponent> shell,
        EntityUid user, out TimeSpan travelTime, out MapCoordinates coordinates)
    {
        travelTime = default;
        coordinates = default;
        if (!mortar.Comp.Deployed || !Transform(mortar).Anchored)
        {
            _popup.PopupEntity(Loc.GetString("rmc-mortar-not-deployed", ("mortar", mortar)), user, user, SmallCaution);
            return false;
        }

        if (mortar.Comp.Loaded || HasComp<ActiveMortarShellComponent>(shell) ||
            (_container.TryGetContainer(mortar, mortar.Comp.ContainerId, out var container) &&
             (container.ContainedEntities.Count > 0 || !_container.CanInsert(shell, container))))
        {
            _popup.PopupEntity(Loc.GetString("humanity-mortar-loaded"), user, user, SmallCaution);
            return false;
        }
        return true;
    }

    protected override bool TryGetShotCoordinates(Entity<MortarComponent> mortar, Entity<MortarShellComponent> shell,
        EntityUid user, out TimeSpan travelTime, out MapCoordinates coordinates)
    {
        travelTime = default;
        coordinates = default;
        if (mortar.Comp.LastFiredAt != TimeSpan.Zero && _timing.CurTime < mortar.Comp.LastFiredAt + mortar.Comp.FireDelay)
        {
            _popup.PopupEntity(Loc.GetString("rmc-mortar-fire-cooldown", ("mortar", mortar)), user, user, SmallCaution);
            return false;
        }
        if (!float.IsFinite(mortar.Comp.Heading) ||
            mortar.Comp.Range < mortar.Comp.MinimumRange || mortar.Comp.Range > mortar.Comp.MaximumRange)
            return false;

        var origin = _transform.GetMapCoordinates(mortar);
        var spread = 0.5f + mortar.Comp.Range * 0.02f;
        coordinates = origin.Offset(GetAimDirection(mortar.Comp.Heading) * mortar.Comp.Range + _random.NextVector2(spread));
        travelTime = TimeSpan.FromSeconds(Math.Max(2, shell.Comp.TravelDelay.TotalSeconds * mortar.Comp.Range / mortar.Comp.MaximumRange));
        return true;
    }
}
