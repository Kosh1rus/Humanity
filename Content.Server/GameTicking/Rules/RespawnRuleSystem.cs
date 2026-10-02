using Content.Server.Chat.Managers;
using Content.Server.Database.Migrations.Postgres;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Station.Systems;
using Content.Shared.Chat;
using Content.Shared.GameTicking.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Players;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using Content.Server.Chat.Systems;

namespace Content.Server.GameTicking.Rules;

/// <summary>
/// This handles logic and interactions related to <see cref="RespawnDeadRuleComponent"/>
/// </summary>
public sealed class RespawnRuleSystem : GameRuleSystem<RespawnDeadRuleComponent>
{
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    private bool _announced = false;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SuicideEvent>(OnSuicide);
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
    }

    protected override void Started(EntityUid uid, RespawnDeadRuleComponent component, GameRuleComponent rule, GameRuleStartedEvent args)
    {
        if (TryComp<RespawnTrackerComponent>(uid, out var tracker))
            tracker.GlobalTimer = _timing.CurTime + tracker.RespawnDelay;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (GameTicker.RunLevel != GameRunLevel.InRound)
            return;

        var query = EntityQueryEnumerator<RespawnDeadRuleComponent, RespawnTrackerComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out _, out var tracker, out var rule))
        {
            if (!GameTicker.IsGameRuleActive(uid, rule))
                continue;
            if (!tracker.Fixed)
            {
                foreach (var (player, time) in tracker.RespawnQueue)
                {
                    if (_timing.CurTime < time)
                        continue;

                    //This autorespawner is disabled since people can manually return to the lobby.
                    //We just check on the spawn event if the player is not in the queue.
                    //if (session.GetMind() is { } mind && TryComp<MindComponent>(mind, out var mindComp) && mindComp.OwnedEntity.HasValue)
                    //    QueueDel(mindComp.OwnedEntity.Value);
                    //GameTicker.MakeJoinGame(session, station, silent: true);
                    tracker.RespawnQueue.Remove(player);
                }
            }
            else
            {
                if (_timing.CurTime > tracker.GlobalTimer && _announced == false)
                {
                    _announced = true;
                    var announcementMessage = "Подкрепления доступны. Вступайте через лобби.";
                    RespawnFixed(tracker);
                    _chat.DispatchGlobalAnnouncement(announcementMessage, "Round", false, null, Color.Yellow);
                    tracker.GlobalTimer = _timing.CurTime + tracker.RespawnDelay;
                    _announced = false;
                }
            }
        }
    }

    private void RespawnFixed(RespawnTrackerComponent tracker)
    {
        tracker.RespawnQueue.Clear();
    }

    private void OnSuicide(SuicideEvent ev)
    {
        if (!TryComp<ActorComponent>(ev.Victim, out var actor))
            return;
        var query = EntityQueryEnumerator<RespawnDeadRuleComponent, RespawnTrackerComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var respawnRule, out var tracker, out var rule))
        {
            if (!GameTicker.IsGameRuleActive(uid, rule))
                continue;

            if (respawnRule.AlwaysRespawnDead)
                AddToTracker(actor.PlayerSession.UserId, (uid, tracker));
        }
    }

    private void OnMobStateChanged(MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        if (!TryComp<ActorComponent>(args.Target, out var actor))
            return;

        var query = EntityQueryEnumerator<RespawnDeadRuleComponent, RespawnTrackerComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var respawnRule, out var tracker, out var rule))
        {
            if (!GameTicker.IsGameRuleActive(uid, rule))
                continue;

            if (respawnRule.AlwaysRespawnDead)
                AddToTracker(actor.PlayerSession.UserId, (uid, tracker));
            if (RespawnPlayer((args.Target, actor), (uid, tracker)))
                break;
        }
    }

    /// <summary>
    /// Attempts to directly respawn a player, skipping the lobby screen.
    /// </summary>
    public bool RespawnPlayer(Entity<ActorComponent> player, Entity<RespawnTrackerComponent> respawnTracker)
    {
        if (!respawnTracker.Comp.Players.Contains(player.Comp.PlayerSession.UserId) || respawnTracker.Comp.RespawnQueue.ContainsKey(player.Comp.PlayerSession.UserId))
            return false;

        if (respawnTracker.Comp.RespawnDelay == TimeSpan.Zero)
        {
            if (_station.GetStations().FirstOrNull() is not { } station)
                return false;

            if (respawnTracker.Comp.DeleteBody)
                QueueDel(player);
            GameTicker.MakeJoinGame(player.Comp.PlayerSession, station, silent: true);
            return false;
        }

        var readyAt = respawnTracker.Comp.Fixed
            ? respawnTracker.Comp.GlobalTimer
            : _timing.CurTime + respawnTracker.Comp.RespawnDelay;
        var seconds = Math.Max(0, Math.Ceiling((readyAt - _timing.CurTime).TotalSeconds));
        var msg = Loc.GetString("rule-respawn-in-seconds", ("second", seconds));
        var wrappedMsg = Loc.GetString("chat-manager-server-wrap-message", ("message", msg));
        _chatManager.ChatMessageToOne(ChatChannel.Server, msg, wrappedMsg, respawnTracker, false, player.Comp.PlayerSession.Channel, Color.LimeGreen);

        respawnTracker.Comp.RespawnQueue[player.Comp.PlayerSession.UserId] = readyAt;

        return true;
    }

    /// <summary>
    /// Adds a given player to the respawn tracker, ensuring that they are respawned if they die.
    /// </summary>
    public void AddToTracker(Entity<ActorComponent?> player, Entity<RespawnTrackerComponent?> respawnTracker)
    {
        if (!Resolve(respawnTracker, ref respawnTracker.Comp) || !Resolve(player, ref player.Comp, false))
            return;

        AddToTracker(player.Comp.PlayerSession.UserId, (respawnTracker, respawnTracker.Comp));
    }

    /// <summary>
    /// Adds a given player to the respawn tracker, ensuring that they are respawned if they die.
    /// </summary>
    public void AddToTracker(NetUserId id, Entity<RespawnTrackerComponent> tracker)
    {
        tracker.Comp.Players.Add(id);
    }
}
