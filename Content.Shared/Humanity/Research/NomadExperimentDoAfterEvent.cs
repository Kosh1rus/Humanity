using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Humanity.Research;

[Serializable, NetSerializable]
public sealed partial class NomadExperimentDoAfterEvent : DoAfterEvent
{
    public NetEntity Material { get; }
    public int Age { get; }

    public NomadExperimentDoAfterEvent(NetEntity material, int age)
    {
        Material = material;
        Age = age;
    }

    public override DoAfterEvent Clone() => this;
}
