using Robust.Shared.Prototypes;

namespace Content.Shared._Stalker.Teleport;

[Prototype]
public sealed partial class MapLoaderPrototype : IPrototype
{
    [IdDataField, ViewVariables]
    public string ID { get; private set; } = default!;

    [DataField]
    public Dictionary<string, string> MapPaths = new();
}
