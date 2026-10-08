using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Farming;

[Serializable, NetSerializable]
public sealed partial class CompostDoAfterEvent : DoAfterEvent
{
    public NetEntity CompostedItem { get; }

    public CompostDoAfterEvent(NetEntity used)
    {
        CompostedItem = used;
    }

    public override DoAfterEvent Clone() => this;
}
