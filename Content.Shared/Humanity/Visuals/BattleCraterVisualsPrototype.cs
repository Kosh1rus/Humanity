using Robust.Shared.Prototypes;

namespace Content.Shared.Humanity.Visuals;

[Prototype]
public sealed partial class BattleCraterVisualsPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public float VerticalCompression;

    [DataField(required: true)]
    public float MergeRadiusFactor;

    [DataField(required: true)]
    public float BaseEdgeRadius;

    [DataField(required: true)]
    public float InteriorDepth;

    [DataField(required: true)]
    public float RimDepth;

    [DataField(required: true)]
    public Color InteriorColor;

    [DataField(required: true)]
    public Color RimColor;

    [DataField(required: true)]
    public Color EdgeColor;
}
