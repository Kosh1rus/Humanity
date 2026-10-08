using Content.Shared.Body.Events;
using Content.Shared.Body.Systems;

namespace Content.Shared.Body;

public sealed partial class BodySystem
{
    private IEnumerable<EntityUid> EnumerateAttachedOrgans(Entity<BodyComponent> body)
    {
        var seen = new HashSet<EntityUid>();
        foreach (var organ in body.Comp.Organs?.ContainedEntities ?? [])
        {
            if (seen.Add(organ))
                yield return organ;
        }

        if (body.Comp.RootContainer == null)
            yield break;

        foreach (var (organ, _) in EntityManager.System<SharedBodySystem>().GetBodyOrgans(body.Owner, body.Comp))
        {
            if (seen.Add(organ))
                yield return organ;
        }
    }

    [SubscribeLocalEvent]
    private void OnLimbOrganAdded(Entity<OrganComponent> organ, ref OrganAddedToBodyEvent args)
    {
        var inserted = new OrganInsertedIntoEvent(organ.Owner);
        RaiseLocalEvent(args.Body, ref inserted);
        var attached = new OrganGotInsertedEvent(args.Body);
        RaiseLocalEvent(organ, ref attached);
    }

    [SubscribeLocalEvent]
    private void OnLimbOrganRemoved(Entity<OrganComponent> organ, ref OrganRemovedFromBodyEvent args)
    {
        var removed = new OrganRemovedFromEvent(organ.Owner);
        RaiseLocalEvent(args.OldBody, ref removed);
        var detached = new OrganGotRemovedEvent(args.OldBody);
        RaiseLocalEvent(organ, ref detached);
    }
}
