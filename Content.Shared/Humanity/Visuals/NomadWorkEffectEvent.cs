using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared.Humanity.Visuals;

public enum NomadWorkEffect : byte
{
    Leaves,
    Wood,
    Building,
    Shake,
}

[Serializable, NetSerializable]
public sealed class NomadWorkEffectEvent(NetCoordinates coordinates, NomadWorkEffect effect) : EntityEventArgs
{
    public NetCoordinates Coordinates = coordinates;
    public NomadWorkEffect Effect = effect;
}
