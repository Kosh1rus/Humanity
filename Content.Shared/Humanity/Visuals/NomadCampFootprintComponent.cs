namespace Content.Shared.Humanity.Visuals;

[RegisterComponent]
public sealed partial class NomadCampFootprintComponent : Component
{
    [DataField]
    public float Radius = 0.65f;

    [DataField]
    public bool Charred;
}
