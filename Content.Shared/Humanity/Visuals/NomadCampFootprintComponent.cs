using Robust.Shared.GameStates;

namespace Content.Shared.Humanity.Visuals;

[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NomadCampFootprintComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Radius = 0.65f;

    [DataField, AutoNetworkedField]
    public bool Charred;

    [DataField, AutoNetworkedField]
    public float Wear;
}
