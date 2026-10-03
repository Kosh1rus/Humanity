using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Humanity.Visuals;

[Serializable, NetSerializable]
public sealed partial class NomadGatherDoAfterEvent : DoAfterEvent
{
    public override DoAfterEvent Clone() => this;
}
