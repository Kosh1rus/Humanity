using Robust.Shared.GameStates;

namespace Content.Shared.Humanity.Visuals;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BattleScarComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Radius = 0.7f;

    [DataField, AutoNetworkedField]
    public int Seed;

    [DataField, AutoNetworkedField]
    public bool Rubble;
}
