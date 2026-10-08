using Robust.Shared.Prototypes;

namespace Content.Shared.Maps;

public sealed partial class GameMapPrototype
{
    [DataField("fixedPreset")]
    public string FixedPreset { get; private set; } = string.Empty;
}
