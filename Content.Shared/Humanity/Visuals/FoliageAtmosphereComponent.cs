namespace Content.Shared.Humanity.Visuals;

[RegisterComponent]
public sealed partial class FoliageAtmosphereComponent : Component
{
    [DataField]
    public bool Enabled = true;

    [DataField]
    public bool ShedLeaves = true;
}
