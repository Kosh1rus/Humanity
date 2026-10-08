using Content.Shared.Humanoid;

namespace Content.Shared.Humanoid.Prototypes;

public sealed partial class HumanoidProfilePrototype
{
    [DataField] public Dictionary<HumanoidVisualLayers, CustomBaseLayerInfo> CustomBaseLayers = new();
}
