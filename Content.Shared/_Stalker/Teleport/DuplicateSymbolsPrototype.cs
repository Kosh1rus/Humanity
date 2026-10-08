using Robust.Shared.Prototypes;

namespace Content.Server._Stalker.Teleports.DuplicateTeleport;

/// <summary>
/// This is a prototype for...
/// </summary>
[Prototype]
public sealed partial class DuplicateSymbolsPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public List<string> Symbols = new();
}
