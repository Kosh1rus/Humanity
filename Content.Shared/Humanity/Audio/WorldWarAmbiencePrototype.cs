using System.Numerics;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared.Humanity.Audio;

[Prototype]
public sealed partial class WorldWarAmbiencePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public WorldWarAmbienceLayer[] Layers = [];

    [DataField]
    public float IndoorGain = 0.25f;
}

[DataDefinition]
public sealed partial class WorldWarAmbienceLayer
{
    [DataField(required: true)]
    public SoundSpecifier Sound = default!;

    /// <summary>
    /// Random delay between starts, in seconds. Ignored for looping sounds.
    /// </summary>
    [DataField]
    public Vector2 Interval = new(30f, 60f);
}
