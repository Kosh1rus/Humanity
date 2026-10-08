using Content.Shared.DoAfter;
using Content.Shared.Gatherable.Components;
using Content.Shared.Humanity.Visuals;

namespace Content.Shared.Gatherable;

public sealed partial class GatherableSystem
{
    [Dependency] private SharedDoAfterSystem _doAfter = default!;

    private void BeginGather(Entity<GatherableComponent> gathered, EntityUid user)
    {
        if (_doAfter.IsRunning(gathered.Comp.ActiveGather))
            return;

        if (gathered.Comp.GatherTime <= 0)
        {
            Gather(gathered.AsNullable(), user);
            return;
        }

        var action = new DoAfterArgs(EntityManager, user, gathered.Comp.GatherTime,
            new NomadGatherDoAfterEvent(), gathered, target: gathered)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = true,
            CancelDuplicate = false,
        };
        if (_doAfter.TryStartDoAfter(action, out var id))
        {
            gathered.Comp.ActiveGather = id;
            RaiseLocalEvent(gathered.Owner, new NomadGatherEffectEvent(NomadWorkEffect.Shake));
        }
    }

    [SubscribeLocalEvent]
    private void OnGatherComplete(Entity<GatherableComponent> gathered, ref NomadGatherDoAfterEvent args)
    {
        gathered.Comp.ActiveGather = null;
        if (args.Handled || args.Cancelled || EntityManager.IsQueuedForDeletion(gathered) ||
            _whitelistSystem.IsWhitelistFailOrNull(gathered.Comp.ToolWhitelist, args.Args.User))
            return;

        args.Handled = true;
        Gather(gathered.AsNullable(), args.Args.User);
    }
}
