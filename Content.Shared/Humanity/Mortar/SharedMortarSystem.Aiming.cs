using System.Numerics;
using Content.Shared.Humanity.Mortar;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Player;

namespace Content.Shared._RMC14.Mortar;

public abstract partial class SharedMortarSystem
{
    private void OnMortarAim(Entity<MortarComponent> mortar, ref MortarAimMessage args)
    {
        if (!mortar.Comp.Deployed || !Transform(mortar).Anchored || !float.IsFinite(args.Heading))
            return;

        mortar.Comp.Heading = (args.Heading % 360 + 360) % 360;
        mortar.Comp.Range = Math.Clamp(args.Range, mortar.Comp.MinimumRange, mortar.Comp.MaximumRange);
        _transform.SetWorldRotation(mortar, Angle.FromDegrees(180 - mortar.Comp.Heading));
        Dirty(mortar);
    }

    private void OnMortarFire(Entity<MortarComponent> mortar, ref MortarFireMessage args)
    {
        if (_net.IsClient)
            return;

        if (!mortar.Comp.Deployed || !Transform(mortar).Anchored)
        {
            _popup.PopupEntity(Loc.GetString("rmc-mortar-not-deployed", ("mortar", mortar)), args.Actor, args.Actor);
            return;
        }

        if (!_container.TryGetContainer(mortar, mortar.Comp.ContainerId, out var container) ||
            container.ContainedEntities.Count == 0)
        {
            mortar.Comp.Loaded = false;
            Dirty(mortar);
            _popup.PopupEntity(Loc.GetString("humanity-mortar-empty"), args.Actor, args.Actor);
            return;
        }

        var shellId = container.ContainedEntities[0];
        if (!TryComp<MortarShellComponent>(shellId, out var shell) ||
            !TryGetShotCoordinates(mortar, (shellId, shell), args.Actor, out var travelTime, out var coordinates))
            return;

        if (!_container.Remove(shellId, container))
            return;

        _transform.DetachEntity(shellId);
        var time = _timing.CurTime;
        AddComp(shellId, new ActiveMortarShellComponent
        {
            Shooter = args.Actor,
            Coordinates = _transform.ToCoordinates(coordinates),
            WarnAt = time + travelTime,
            ImpactWarnAt = time + travelTime + shell.ImpactWarningDelay,
            LandAt = time + travelTime + shell.ImpactDelay,
        }, true);
        mortar.Comp.Loaded = false;
        mortar.Comp.LastFiredAt = time;
        Dirty(mortar);
        _audio.PlayPvs(mortar.Comp.FireSound, mortar);
        _popup.PopupEntity(Loc.GetString("rmc-mortar-shell-fire"), mortar);
        RaiseNetworkEvent(new MortarFiredEvent(GetNetEntity(mortar)), Filter.Pvs(mortar));
    }

    protected virtual bool TryGetShotCoordinates(Entity<MortarComponent> mortar,
        Entity<MortarShellComponent> shell, EntityUid user, out TimeSpan travelTime, out MapCoordinates coordinates)
    {
        travelTime = default;
        coordinates = default;
        return false;
    }

    public static Vector2 GetAimDirection(float heading)
    {
        var radians = MathHelper.DegreesToRadians(heading);
        return new Vector2(MathF.Sin(radians), MathF.Cos(radians));
    }
}
