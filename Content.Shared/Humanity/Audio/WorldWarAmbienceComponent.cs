using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Humanity.Audio;

/// <summary>
/// Enables the distant front soundscape on a World War II map.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class WorldWarAmbienceComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public ProtoId<WorldWarAmbiencePrototype> Soundscape;
}
