namespace Content.Shared.Maps;

public sealed partial class ContentTileDefinition
{
    [DataField("biome")]
    public string Biome { get; private set; } = "Temperate";

    [DataField("suffix")]
    public string Suffix { get; private set; } = string.Empty;
}
