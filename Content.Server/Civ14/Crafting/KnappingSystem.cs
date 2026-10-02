using Content.Server.DoAfter;
using Content.Shared.Rocks;
using Content.Shared.Interaction;
using Content.Shared.DoAfter;
using Robust.Server.GameObjects;
using Content.Shared.Popups;
using Content.Shared.Hands.EntitySystems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Server.Rocks;

public sealed partial class KnappingSystem : EntitySystem
{
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<KnappingComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<KnappingComponent, KnappingDoAfterEvent>(OnDoAfter);
    }

    private void OnAfterInteract(EntityUid uid, KnappingComponent component, AfterInteractEvent args)
    {
        if (args.Target == null || !args.CanReach || !HasComp<KnappingAnchoredComponent>(args.Target))
            return;

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, component.HitTime, new KnappingDoAfterEvent(), uid, target: args.Target, used: uid)
        {
            Broadcast = true,
            BreakOnMove = true,
            NeedHand = true,
        });
    }

    private void OnDoAfter(EntityUid flintUid, KnappingComponent component, ref KnappingDoAfterEvent args)
    {

        if (args.Cancelled || args.Handled)
            return;

        component.CurrentHits++;
        var coordinates = args.Args.Target is { } target && Exists(target)
            ? Transform(target).Coordinates
            : Transform(args.Args.User).Coordinates;
        Spawn("EffectSparks", coordinates);
        _audio.PlayPvs("/Audio/Items/Mining/pickaxe.ogg", coordinates,
            AudioParams.Default.WithVolume(-8f).WithVariation(0.1f).WithMaxDistance(8f));

        if (component.CurrentHits >= component.HitsRequired)
        {
            var result = Spawn(component.ResultPrototype, Transform(flintUid).MapPosition);
            QueueDel(flintUid);
            _hands.TryPickupAnyHand(args.Args.User, result);
        }

        args.Handled = true;
    }
}
