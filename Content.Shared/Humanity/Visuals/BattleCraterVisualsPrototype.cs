using System.Numerics;
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

    [DataField]
    public float InteriorRadiusFactor = 0.8f;

    [DataField]
    public float BodyDepth = 0.18f;

    public bool Contains(Vector2 offset, float radius, float? radiusFactor = null)
    {
        if (radius <= 0)
            return false;
        offset.Y *= VerticalCompression;
        var factor = radiusFactor ?? InteriorRadiusFactor;
        return offset.LengthSquared() < radius * radius * factor * factor;
    }
}
