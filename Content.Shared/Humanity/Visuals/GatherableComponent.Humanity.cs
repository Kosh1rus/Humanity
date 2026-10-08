using Content.Shared.DoAfter;

namespace Content.Shared.Gatherable.Components;

public sealed partial class GatherableComponent
{
    [DataField]
    public float GatherTime;

    public DoAfterId? ActiveGather;
}
