using Content.Shared.Hands.Components;

namespace Content.Shared.Hands.EntitySystems;

public abstract partial class SharedHandsSystem
{
    public void AddHand(EntityUid uid, string name, HandLocation location, HandsComponent hands)
        => AddHand((uid, hands), name, location);
}
