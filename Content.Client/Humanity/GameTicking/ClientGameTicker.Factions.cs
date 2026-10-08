using Content.Shared.GameTicking;
using Content.Shared.NPC.Prototypes;
using Robust.Shared.Prototypes;
namespace Content.Client.GameTicking;
public sealed partial class ClientGameTicker
{
    public Dictionary<ProtoId<NpcFactionPrototype>, int> PlayerFactionCounts { get; private set; } = new();
    public event Action? PlayerFactionCountsUpdated;
    [SubscribeNetworkEvent]
    private void OnPlayerFactionCountsReceived(GetPlayerFactionCounts ev)
    {
        PlayerFactionCounts = ev.FactionCounts;
        PlayerFactionCountsUpdated?.Invoke();
    }
}
