using Content.Shared._RMC14.Marines.Roles.Ranks;
using Robust.Shared.Prototypes;

namespace Content.Shared.Roles;

public sealed partial class JobPrototype
{
    [DataField("originalName")]
    public string OriginalName { get; private set; } = string.Empty;

    [DataField("faction")]
    public string Faction { get; private set; } = string.Empty;

    [DataField]
    public List<ProtoId<StartingGearPrototype>>? RandomStartingGears { get; private set; } = new();

    [DataField]
    public Dictionary<ProtoId<RankPrototype>, HashSet<JobRequirement>?>? Ranks;
}
