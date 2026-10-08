using Robust.Shared.Serialization;

namespace Content.Shared.Humanity.Mortar;

[Serializable, NetSerializable]
public sealed class MortarAimMessage(float heading, int range) : BoundUserInterfaceMessage
{
    public readonly float Heading = heading;
    public readonly int Range = range;
}

[Serializable, NetSerializable]
public sealed class MortarFireMessage : BoundUserInterfaceMessage;
