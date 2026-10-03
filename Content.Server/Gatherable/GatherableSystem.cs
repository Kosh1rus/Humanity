using Content.Server.Destructible;
using Content.Server.Gatherable.Components;
using Content.Shared.Interaction;
using Content.Shared.Tag;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Whitelist;
using Content.Shared.DoAfter;
using Content.Shared.Humanity.Visuals;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Gatherable;

public sealed partial class GatherableSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly DestructibleSystem _destructible = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GatherableComponent, ActivateInWorldEvent>(OnActivate);
        SubscribeLocalEvent<GatherableComponent, AttackedEvent>(OnAttacked);
        SubscribeLocalEvent<GatherableComponent, NomadGatherDoAfterEvent>(OnGatherComplete);
        InitializeProjectile();
    }

    private void OnAttacked(Entity<GatherableComponent> gatherable, ref AttackedEvent args)
    {
        if (_whitelistSystem.IsWhitelistFailOrNull(gatherable.Comp.ToolWhitelist, args.Used))
            return;

        Gather(gatherable, args.User);
    }

    private void OnActivate(Entity<GatherableComponent> gatherable, ref ActivateInWorldEvent args)
    {
        if (args.Handled || !args.Complex)
            return;

        if (_whitelistSystem.IsWhitelistFailOrNull(gatherable.Comp.ToolWhitelist, args.User))
            return;

        if (gatherable.Comp.Gathering)
        {
            args.Handled = true;
            return;
        }
        if (gatherable.Comp.GatherTime <= 0)
            Gather(gatherable, args.User);
        else
        {
            var action = new DoAfterArgs(EntityManager, args.User, gatherable.Comp.GatherTime,
                new NomadGatherDoAfterEvent(), gatherable.Owner, target: gatherable.Owner)
            {
                BreakOnMove = true,
                BreakOnDamage = true,
                NeedHand = true,
            };
            gatherable.Comp.Gathering = _doAfter.TryStartDoAfter(action);
            if (gatherable.Comp.Gathering)
                EntityManager.System<Content.Server.Humanity.Visuals.NomadLandscapeSystem>()
                    .Emit(gatherable.Owner, NomadWorkEffect.Shake);
        }
        args.Handled = true;
    }

    private void OnGatherComplete(EntityUid uid, GatherableComponent component, ref NomadGatherDoAfterEvent args)
    {
        component.Gathering = false;
        if (args.Handled || args.Cancelled || EntityManager.IsQueuedForDeletion(uid) ||
            _whitelistSystem.IsWhitelistFailOrNull(component.ToolWhitelist, args.Args.User))
            return;
        args.Handled = true;
        Gather(uid, args.Args.User, component);
    }

    public void Gather(EntityUid gatheredUid, EntityUid? gatherer = null, GatherableComponent? component = null)
    {
        if (!Resolve(gatheredUid, ref component) || EntityManager.IsQueuedForDeletion(gatheredUid))
            return;

        if (TryComp<SoundOnGatherComponent>(gatheredUid, out var soundComp))
        {
            _audio.PlayPvs(soundComp.Sound, Transform(gatheredUid).Coordinates);
        }

        var pos = _transform.GetMapCoordinates(gatheredUid);
        var effect = TryComp<Content.Shared.Humanity.Visuals.FoliageAtmosphereComponent>(gatheredUid, out var foliage) && foliage.ShedLeaves
            ? Content.Shared.Humanity.Visuals.NomadWorkEffect.Wood
            : Content.Shared.Humanity.Visuals.NomadWorkEffect.Leaves;
        EntityManager.System<Content.Server.Humanity.Visuals.NomadLandscapeSystem>().Emit(gatheredUid, effect);
        _destructible.DestroyEntity(gatheredUid);

        // Spawn the loot!
        if (component.Loot == null)
            return;

        foreach (var (tag, table) in component.Loot)
        {
            if (tag != "All")
            {
                if (gatherer != null && !_tagSystem.HasTag(gatherer.Value, tag))
                    continue;
            }
            var getLoot = _proto.Index(table);
            var spawnLoot = getLoot.GetSpawns(_random);
            foreach (var loot in spawnLoot)
            {
                var spawnPos = pos.Offset(_random.NextVector2(component.GatherOffset));
                Spawn(loot, spawnPos);
            }
        }
    }
}
