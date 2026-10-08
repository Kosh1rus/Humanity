using System.Linq;
using Content.Shared.Humanity.Mortar;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Administration.Logs;
using Content.Shared.Construction.Components;
using Content.Shared.Coordinates;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.UserInterface;
using Content.Shared.Explosion.EntitySystems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._RMC14.Mortar;

public abstract partial class SharedMortarSystem : EntitySystem
{
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private FixtureSystem _fixture = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private ISharedPlayerManager _player = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedExplosionSystem _explosion = default!;

    private EntityQuery<TransformComponent> _transformQuery;

    public override void Initialize()
    {
        _transformQuery = GetEntityQuery<TransformComponent>();

        SubscribeLocalEvent<MortarComponent, UseInHandEvent>(OnMortarUseInHand, before: [typeof(ActivatableUISystem)]);
        SubscribeLocalEvent<MortarComponent, DeployMortarDoAfterEvent>(OnMortarDeployDoAfter);
        SubscribeLocalEvent<MortarComponent, InteractUsingEvent>(OnMortarInteractUsing);
        SubscribeLocalEvent<MortarComponent, LoadMortarShellDoAfterEvent>(OnMortarLoadDoAfter);
        SubscribeLocalEvent<MortarComponent, UnanchorAttemptEvent>(OnMortarUnanchorAttempt);
        SubscribeLocalEvent<MortarComponent, AnchorStateChangedEvent>(OnMortarAnchorStateChanged);
        SubscribeLocalEvent<MortarComponent, ExaminedEvent>(OnMortarExamined);
        SubscribeLocalEvent<MortarComponent, ActivatableUIOpenAttemptEvent>(OnMortarActivatableUIOpenAttempt);
        SubscribeLocalEvent<MortarComponent, CombatModeShouldHandInteractEvent>(OnMortarShouldInteract);
        SubscribeLocalEvent<MortarComponent, DestructionEventArgs>(OnMortarDestruction);
        SubscribeLocalEvent<MortarComponent, BeforeDamageChangedEvent>(OnMortarBeforeDamageChanged);

        Subs.BuiEvents<MortarComponent>(MortarUiKey.Key,
            subs =>
            {
                subs.Event<MortarAimMessage>(OnMortarAim);
                subs.Event<MortarFireMessage>(OnMortarFire);
            });
    }

    private void OnMortarBeforeDamageChanged(Entity<MortarComponent> ent, ref BeforeDamageChangedEvent args)
    {
        if (!ent.Comp.Deployed) // cannot destroy in item form
            args.Cancelled = true;
    }

    private void OnMortarDestruction(Entity<MortarComponent> mortar, ref DestructionEventArgs args)
    {
        if (!mortar.Comp.Deployed || _net.IsClient)
            return;

        SpawnAtPosition(mortar.Comp.Drop, mortar.Owner.ToCoordinates());
    }

    private void OnMortarUseInHand(Entity<MortarComponent> mortar, ref UseInHandEvent args)
    {
        args.Handled = true;
        DeployMortar(mortar, args.User);
    }

    private void OnMortarDeployDoAfter(Entity<MortarComponent> mortar, ref DeployMortarDoAfterEvent args)
    {
        var user = args.User;
        if (args.Cancelled || args.Handled)
            return;

        args.Handled = true;
        if (mortar.Comp.Deployed)
            return;

        if (!CanDeployPopup(mortar, user))
            return;

        if (_hands.IsHolding(user, mortar, out _) && !_hands.TryDrop(user, mortar))
            return;

        var xform = Transform(mortar);
        var coordinates = _transform.GetMoverCoordinates(mortar, xform);
        _transform.SetCoordinates(mortar, xform, coordinates);
        if (!_transform.AnchorEntity((mortar.Owner, xform)))
        {
            _popup.PopupClient(Loc.GetString("humanity-mortar-no-ground"), user, user);
            return;
        }

        mortar.Comp.Deployed = true;
        mortar.Comp.Heading = (float) ((180 - _transform.GetWorldRotation(user).Degrees + 360) % 360);
        mortar.Comp.Range = Math.Clamp(mortar.Comp.Range, mortar.Comp.MinimumRange, mortar.Comp.MaximumRange);
        _transform.SetWorldRotation(mortar, Angle.FromDegrees(180 - mortar.Comp.Heading));
        Dirty(mortar);

        if (_fixture.GetFixtureOrNull(mortar, mortar.Comp.FixtureId) is { } fixture)
            _physics.SetHard(mortar, fixture, true);

        _appearance.SetData(mortar, MortarVisualLayers.State, MortarVisuals.Deployed);

        _audio.PlayPredicted(mortar.Comp.DeploySound, mortar, user);
    }

    private void OnMortarInteractUsing(Entity<MortarComponent> mortar, ref InteractUsingEvent args)
    {
        var shellId = args.Used;
        if (!TryComp(shellId, out MortarShellComponent? shell))
            return;

        args.Handled = true;
        var user = args.User;

        if (!CanLoadPopup(mortar, (shellId, shell), user, out _, out _))
            return;

        var ev = new LoadMortarShellDoAfterEvent();
        var doAfter = new DoAfterArgs(EntityManager, user, shell.LoadDelay, ev, mortar, mortar, shellId)
        {
            BreakOnMove = true,
            BreakOnHandChange = true,
            DuplicateCondition = DuplicateConditions.SameTarget,
        };

        if (_doAfter.TryStartDoAfter(doAfter))
        {
            var selfMsg = Loc.GetString("rmc-mortar-shell-load-start-self", ("mortar", mortar), ("shell", shellId));
            var othersMsg = Loc.GetString("rmc-mortar-shell-load-start-others",
                ("user", user),
                ("mortar", mortar),
                ("shell", shellId));
            _popup.PopupPredicted(selfMsg, othersMsg, mortar, user);

            _audio.PlayPredicted(mortar.Comp.ReloadSound, mortar, user);
        }
    }

    private void OnMortarLoadDoAfter(Entity<MortarComponent> mortar, ref LoadMortarShellDoAfterEvent args)
    {
        var user = args.User;
        if (args.Cancelled || args.Handled || args.Used is not { } shellId)
            return;

        args.Handled = true;
        if (_net.IsClient)
            return;

        if (!TryComp(shellId, out MortarShellComponent? shell))
            return;

        if (!mortar.Comp.Deployed)
            return;

        if (HasComp<ActiveMortarShellComponent>(shellId))
            return;

        if (!CanLoadPopup(mortar, (shellId, shell), user, out _, out _))
            return;

        var container = _container.EnsureContainer<ContainerSlot>(mortar, mortar.Comp.ContainerId);
        if (!_container.Insert(shellId, container))
            return;

        mortar.Comp.Loaded = true;
        Dirty(mortar);

        var selfMsg = Loc.GetString("rmc-mortar-shell-load-finish-self", ("mortar", mortar), ("shell", shellId));
        var othersMsg = Loc.GetString("rmc-mortar-shell-load-finish-others", ("user", user), ("mortar", mortar), ("shell", shellId));
        _popup.PopupPredicted(selfMsg, othersMsg, user, user);

    }

    private void OnMortarUnanchorAttempt(Entity<MortarComponent> mortar, ref UnanchorAttemptEvent args)
    {
        if (args.Cancelled)
            return;
    }

    private void OnMortarAnchorStateChanged(Entity<MortarComponent> mortar, ref AnchorStateChangedEvent args)
    {
        if (args.Anchored)
            return;

        mortar.Comp.Deployed = false;
        if (!_net.IsClient && _container.TryGetContainer(mortar, mortar.Comp.ContainerId, out var container))
        {
            foreach (var shell in container.ContainedEntities.ToArray())
                _container.Remove(shell, container);
            mortar.Comp.Loaded = false;
        }
        Dirty(mortar);

        if (_fixture.GetFixtureOrNull(mortar, mortar.Comp.FixtureId) is { } fixture)
            _physics.SetHard(mortar, fixture, false);

        _appearance.SetData(mortar, MortarVisualLayers.State, MortarVisuals.Item);
    }

    private void OnMortarExamined(Entity<MortarComponent> ent, ref ExaminedEvent args)
    {
        using (args.PushGroup(nameof(MortarComponent)))
        {
            args.PushMarkup(Loc.GetString("rmc-mortar-less-accurate-with-range"));
        }
    }

    private void OnMortarActivatableUIOpenAttempt(Entity<MortarComponent> ent, ref ActivatableUIOpenAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (!ent.Comp.Deployed || !Transform(ent).Anchored)
            args.Cancel();
    }

    private void OnMortarShouldInteract(Entity<MortarComponent> ent, ref CombatModeShouldHandInteractEvent args)
    {
        args.Cancelled = true;
    }

    private void DeployMortar(Entity<MortarComponent> mortar, EntityUid user)
    {
        if (mortar.Comp.Deployed)
            return;

        if (!CanDeployPopup(mortar, user))
            return;

        var ev = new DeployMortarDoAfterEvent();
        var args = new DoAfterArgs(EntityManager, user, mortar.Comp.DeployDelay, ev, mortar)
        {
            BreakOnMove = true,
            BreakOnHandChange = true,
        };

        if (_doAfter.TryStartDoAfter(args))
            _popup.PopupClient(Loc.GetString("rmc-mortar-deploy-start", ("mortar", mortar)), user, user);
    }

    private bool CanDeployPopup(Entity<MortarComponent> mortar, EntityUid user)
    {
        if (Transform(user).GridUid == null)
        {
            _popup.PopupClient(Loc.GetString("humanity-mortar-no-ground"), user, user);
            return false;
        }

        return true;
    }

    protected virtual bool CanLoadPopup(
        Entity<MortarComponent> mortar,
        Entity<MortarShellComponent> shell,
        EntityUid user,
        out TimeSpan travelTime,
        out MapCoordinates coordinates)
    {
        travelTime = default;
        coordinates = default;
        return false;
    }

    public void PopupWarning(MapCoordinates coordinates, float range, LocId warning, LocId warningAbove)
    {
        foreach (var session in _player.NetworkedSessions)
        {
            if (session.AttachedEntity is not { } recipient ||
                !_transformQuery.TryComp(recipient, out var xform) ||
                xform.MapID != coordinates.MapId)
            {
                continue;
            }

            var sessionCoordinates = _transform.GetMapCoordinates(xform);
            var distanceVec = (coordinates.Position - sessionCoordinates.Position);
            var distance = distanceVec.Length();
            if (distance > range)
                continue;

            var direction = distanceVec.GetDir().ToString().ToUpperInvariant();
            var msg = distance < 1
                ? Loc.GetString(warningAbove)
                : Loc.GetString(warning, ("direction", direction));
            _popup.PopupEntity(msg, recipient, recipient, PopupType.LargeCaution);
        }
    }

    public override void Update(float frameTime)
    {
        if (_net.IsClient)
            return;

        var time = _timing.CurTime;
        var shells = EntityQueryEnumerator<ActiveMortarShellComponent>();
        while (shells.MoveNext(out var uid, out var active))
        {
            if (!active.Warned && time >= active.WarnAt)
            {
                active.Warned = true;
                var coordinates = _transform.ToMapCoordinates(active.Coordinates);
                //PopupWarning(coordinates,
                //    active.WarnRange,
                //    "rmc-mortar-shell-warning",
                //    "rmc-mortar-shell-warning-above");
                _audio.PlayPvs(active.WarnSound, active.Coordinates);
            }

            if (!active.ImpactWarned && time >= active.ImpactWarnAt)
            {
                active.ImpactWarned = true;
                var coordinates = _transform.ToMapCoordinates(active.Coordinates);
                //PopupWarning(coordinates,
                //    active.WarnRange,
                //    "rmc-mortar-shell-impact-warning",
                //    "rmc-mortar-shell-impact-warning-above");
            }

            if (time >= active.LandAt)
            {
                RemComp<ActiveMortarShellComponent>(uid);
                _transform.SetCoordinates(uid, active.Coordinates);

                var ev = new MortarShellLandEvent(active.Coordinates);
                RaiseLocalEvent(uid, ref ev);

                _explosion.TriggerExplosive(uid, user: active.Shooter);
            }
        }
    }
}
