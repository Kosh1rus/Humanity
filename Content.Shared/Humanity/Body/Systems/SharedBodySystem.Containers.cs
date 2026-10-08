using Robust.Shared.Containers;
using Robust.Shared.GameStates;

namespace Content.Shared.Body.Systems;

[RegisterComponent, NetworkedComponent]
public sealed partial class LimbBodyComponent : Component;

public abstract partial class SharedBodySystem
{
    private void OnLimbBodyInserted(Entity<LimbBodyComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (TryComp<BodyComponent>(ent, out var body))
            OnBodyInserted((ent.Owner, body), ref args);
    }

    private void OnLimbBodyRemoved(Entity<LimbBodyComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (TryComp<BodyComponent>(ent, out var body))
            OnBodyRemoved((ent.Owner, body), ref args);
    }
}
