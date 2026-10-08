using Robust.Shared.Audio;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;


/// <summary>
/// This is used for tagging a mob as hostile.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HostileHUDComponent : Component
{

    /// <summary>
    ///
    /// </summary>
    [DataField("statusIcon")]
    public string StatusIcon = "HostileFaction";
}
