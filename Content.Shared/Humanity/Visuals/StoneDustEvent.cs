using Robust.Shared.Serialization;
using Robust.Shared.Map;

namespace Content.Shared.Humanity.Visuals;

[Serializable, NetSerializable]
public sealed class StoneDustEvent(NetCoordinates coordinates) : EntityEventArgs
{
    public NetCoordinates Coordinates = coordinates;
}
