namespace Content.Shared.Humanity.Research;

[RegisterComponent]
public sealed partial class NomadResearchTableComponent : Component
{
    [DataField]
    public float ExperimentDuration = 30f;

    [DataField]
    public float ResearchPoints = 5f;

    [ViewVariables]
    public bool Busy;
}
