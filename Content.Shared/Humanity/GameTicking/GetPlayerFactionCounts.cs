using Content.Shared.NPC.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
namespace Content.Shared.GameTicking;
[Serializable, NetSerializable]
public sealed class GetPlayerFactionCounts(Dictionary<ProtoId<NpcFactionPrototype>, int> counts) : EntityEventArgs
{
    public Dictionary<ProtoId<NpcFactionPrototype>, int> FactionCounts { get; } = counts;
}
