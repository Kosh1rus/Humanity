using Content.Shared.Construction.Prototypes;
using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;

namespace Content.Server.Construction.Components
{
    [RegisterComponent, Access(typeof(ConstructionSystem))]
    public sealed partial class ConstructionComponent : Component
    {
        [DataField(required: true)]
        public ProtoId<ConstructionGraphPrototype> Graph;

        [DataField(required: true)]
        public string Node = string.Empty;

        [DataField("edge")]
        public int? EdgeIndex;

        [DataField("step")]
        public int StepIndex;

        [DataField]
        public HashSet<string> Containers = new();

        [DataField("defaultTarget")]
        public string? TargetNode;

        [ViewVariables]
        public int? TargetEdgeIndex;

        [ViewVariables]
        public Queue<string>? NodePathfinding;

        [DataField("deconstructionTarget")]
        public string? DeconstructionNode = "start";

        [DataField("cost")]
        public int BuildCost { get; set; } = 0;

        [DataField("time")]
        public int BuildTime { get; set; } = 0;

        [DataField("material")]
        public string BuildMaterial { get; set; } = "WooodPlank";

        [DataField("agemax")]
        public int AgeMax { get; set; } = 8;

        [DataField("agemin")]
        public int AgeMin { get; set; } = 0;

        [ViewVariables]
        // TODO Force flush interaction queue before serializing to YAML.
        // Otherwise you can end up with entities stuck in invalid states (e.g., waiting for DoAfters).
        public readonly Queue<object> InteractionQueue = new();
    }
}
