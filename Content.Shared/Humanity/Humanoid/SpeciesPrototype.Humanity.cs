namespace Content.Shared.Humanoid.Prototypes;

public sealed partial class SpeciesPrototype
{
    [DataField("sprites")] public string SpriteSet { get; private set; } = string.Empty;
    [DataField("markingLimits")] public string MarkingPoints { get; private set; } = string.Empty;
}
