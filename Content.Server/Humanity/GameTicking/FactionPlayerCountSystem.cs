using Content.Shared.GameTicking;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Prototypes;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
namespace Content.Server.Humanity.GameTicking;
public sealed partial class FactionPlayerCountSystem : EntitySystem
{
    [Dependency] private ISharedPlayerManager _players = default!;
    [Dependency] private ServerGameTicker _ticker = default!;
    private float _elapsed;
    public override void Update(float frameTime)
    {
        _elapsed += frameTime;
        if (_elapsed < 5f)
            return;
        _elapsed = 0f;
        RaiseNetworkEvent(new GetPlayerFactionCounts(GetCounts()));
    }

    public Dictionary<ProtoId<NpcFactionPrototype>, int> GetCounts()
    {
        var counts = new Dictionary<ProtoId<NpcFactionPrototype>, int>();
        var matches = EntityQueryEnumerator<TeamDeathMatchRuleComponent, GameRuleComponent>();
        while (matches.MoveNext(out var uid, out var match, out var rule))
        {
            if (!_ticker.IsGameRuleActive((uid, rule)))
                continue;
            if (match.Team1 != "")
                counts.TryAdd(match.Team1, 0);
            if (match.Team2 != "")
                counts.TryAdd(match.Team2, 0);
        }
        foreach (var session in _players.Sessions)
        {
            if (session.AttachedEntity is not { } player || !TryComp<NpcFactionMemberComponent>(player, out var member))
                continue;
            foreach (var faction in member.Factions)
                counts[faction] = counts.GetValueOrDefault(faction) + 1;
        }
        return counts;
    }

    public bool CanJoin(ProtoId<NpcFactionPrototype> faction, out int ownCount, out int otherCount)
    {
        ownCount = 0;
        otherCount = 0;
        var matches = EntityQueryEnumerator<TeamDeathMatchRuleComponent, GameRuleComponent>();
        while (matches.MoveNext(out var uid, out var match, out var rule))
        {
            if (!_ticker.IsGameRuleActive((uid, rule)) || match.Team1 == "" || match.Team2 == "")
                continue;
            var other = faction == match.Team1 ? match.Team2 : faction == match.Team2 ? match.Team1 : null;
            if (other == null)
                continue;
            var counts = GetCounts();
            ownCount = counts.GetValueOrDefault(faction);
            otherCount = counts.GetValueOrDefault(other);
            return ownCount <= otherCount;
        }
        return true;
    }
}
