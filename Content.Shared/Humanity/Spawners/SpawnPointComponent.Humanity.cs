using Content.Shared.NPC.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.Spawners.Components;

public sealed partial class SpawnPointComponent
{
    [DataField]
    public ProtoId<NpcFactionPrototype>? Faction;
}
