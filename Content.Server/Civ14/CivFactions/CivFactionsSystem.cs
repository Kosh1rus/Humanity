using Content.Server.Chat.Systems;
using Content.Shared.Civ14.CivFactions;
using Content.Shared.Popups; // Use Shared Popups
using Robust.Server.Player;
using Robust.Shared.Player; // Required for Filter, ICommonSession
using Robust.Shared.Network; // Required for NetUserId, INetChannel
using System.Linq;
using Content.Server.Chat.Managers;
using Content.Shared.Chat;
using Robust.Shared.Map.Components;
using Robust.Shared.GameObjects; // Required for EntityUid
using Content.Server.GameTicking;
using Content.Shared.GameTicking;
using Content.Shared.Humanity.Factions;
using Robust.Shared.Timing;
using Content.Shared.Weather;

namespace Content.Server.Civ14.CivFactions;

public sealed class CivFactionsSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!; // Use IEntityManager
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    private readonly Dictionary<NetUserId, (FactionData Faction, TimeSpan Expires)> _invites = new();
    private EntityUid? _factionsEntity;
    private CivFactionsComponent? _factionsComponent;

    /// <summary>
    /// Initialises the faction system, ensuring the global factions component exists and subscribing to relevant network events for faction management.
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();

        // Attempt to find the global factions component on startup
        EnsureFactionsComponent();

        // Subscribe to network events
        SubscribeNetworkEvent<CreateFactionRequestEvent>(OnCreateFactionRequest);
        SubscribeNetworkEvent<FactionListRequestEvent>(OnFactionListRequest);
        SubscribeNetworkEvent<LeaveFactionRequestEvent>(OnLeaveFactionRequest);
        SubscribeNetworkEvent<InviteFactionRequestEvent>(OnInviteFactionRequest);
        SubscribeNetworkEvent<AcceptFactionInviteEvent>(OnAcceptFactionInvite);
        SubscribeNetworkEvent<ManageFactionMemberEvent>(OnManageMember);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnSpawnComplete);
    }

    /// <summary>
    /// Performs cleanup operations when the faction system is shut down.
    /// </summary>
    public override void Shutdown()
    {
        base.Shutdown();
    }

    /// <summary>
    /// Ensures the global CivFactionsComponent exists and caches its reference.
    /// Creates one if necessary (e.g., attached to the first map found).
    /// <summary>
    /// Ensures that a global CivFactionsComponent exists and is cached, creating one on a map entity if necessary.
    /// </summary>
    /// <returns>True if the factions component is available and cached; false if it could not be ensured.</returns>
    private bool EnsureFactionsComponent()
    {
        if ((_gameTicker.CurrentPreset ?? _gameTicker.Preset)?.ID == "TDMWW2" ||
            !_gameTicker.IsGameRuleActive("FactionRule"))
        {
            Log.Info($"Factions are disabled on this map.");
            return false;
        }
        if (_factionsComponent != null && !_entityManager.Deleted(_factionsEntity))
            return true; // Already cached and valid

        _invites.Clear();
        var query = EntityQueryEnumerator<CivFactionsComponent>();
        if (query.MoveNext(out var owner, out var comp))
        {
            _factionsEntity = owner;
            _factionsComponent = comp;
            foreach (var faction in comp.FactionList)
            {
                if (faction.FactionLeaders.Count == 0 && faction.FactionMembers.Count > 0)
                    faction.FactionLeaders.Add(faction.FactionMembers[0]);
            }
            Log.Info($"Found existing CivFactionsComponent on entity {_entityManager.ToPrettyString(owner)}");
            return true;
        }
        else
        {
            var mapQuery = EntityQueryEnumerator<MapComponent, WeatherNomadsComponent>();
            if (mapQuery.MoveNext(out var mapUid, out _, out _))
            {
                Log.Info($"No CivFactionsComponent found. Creating one on map entity {_entityManager.ToPrettyString(mapUid)}.");
                _factionsComponent = _entityManager.AddComponent<CivFactionsComponent>(mapUid);
                _factionsEntity = mapUid;
                return true;
            }
            else
            {
                Log.Error("Could not find CivFactionsComponent and no map entity found to attach a new one!");
                _factionsComponent = null;
                _factionsEntity = null;
                return false;
            }
        }
    }

    /// <summary>
    /// Handles a request to create a new faction, validating the faction name and player status, and adds the player as the initial member if successful.
    /// </summary>

    private void OnFactionListRequest(FactionListRequestEvent msg, EntitySessionEventArgs args)
    {
        var factions = new Dictionary<string, List<string>>();
        if (EnsureFactionsComponent() && _factionsComponent != null)
        {
            foreach (var faction in _factionsComponent.FactionList.OrderBy(f => f.FactionName))
            {
                var names = new List<string>();
                foreach (var member in faction.FactionMembers)
                {
                    var session = _playerManager.Sessions.FirstOrDefault(p => p.UserId.ToString() == member);
                    var name = session?.Name ?? "Участник вне сети";
                    if (faction.FactionLeaders.Contains(member))
                        name += " — глава";
                    names.Add(name);
                }
                factions[faction.FactionName] = names;
            }
        }
        RaiseNetworkEvent(new FactionListResponseEvent(factions), args.SenderSession.Channel);
    }

    private void OnCreateFactionRequest(CreateFactionRequestEvent msg, EntitySessionEventArgs args)
    {
        if (!EnsureFactionsComponent())
        {
            return;
        }
        var sourceEntity = _factionsEntity ?? EntityUid.Invalid; // Use Invalid if component entity is somehow null

        if (_factionsComponent == null || _factionsEntity == null)
        {
            Log.Error($"Player {args.SenderSession.Name} tried to create faction, but CivFactionsComponent is missing!");
            // FIX: Correct arguments for ChatMessageToOne
            var errorMsg = "Не удалось создать фракцию из-за ошибки сервера.";
            _chatManager.ChatMessageToOne(ChatChannel.Notifications, errorMsg, errorMsg, sourceEntity, false, args.SenderSession.Channel);
            return;
        }

        var playerSession = args.SenderSession;
        var playerId = playerSession.UserId.ToString();

        // Validation
        if (string.IsNullOrWhiteSpace(msg.FactionName) || msg.FactionName.Length > 32 || msg.FactionName != msg.FactionName.Trim() || msg.FactionName.Any(c => !char.IsLetterOrDigit(c) && c != ' ' && c != '-' && c != '_'))
        {
            // FIX: Correct arguments for ChatMessageToOne
            var errorMsg = "Недопустимое название фракции.";
            _chatManager.ChatMessageToOne(ChatChannel.Notifications, errorMsg, errorMsg, sourceEntity, false, playerSession.Channel);
            return;
        }

        if (IsPlayerInFaction(playerSession.UserId, out _))
        {
            // FIX: Correct arguments for ChatMessageToOne
            var errorMsg = "Вы уже состоите во фракции.";
            _chatManager.ChatMessageToOne(ChatChannel.Notifications, errorMsg, errorMsg, sourceEntity, false, playerSession.Channel);
            return;
        }

        if (_factionsComponent.FactionList.Any(f => f.FactionName.Equals(msg.FactionName, StringComparison.OrdinalIgnoreCase)))
        {
            // FIX: Correct arguments for ChatMessageToOne
            var errorMsg = $"Фракция с названием «{msg.FactionName}» уже существует.";
            _chatManager.ChatMessageToOne(ChatChannel.Notifications, errorMsg, errorMsg, sourceEntity, false, playerSession.Channel);
            return;
        }

        // Create the new faction component
        var newFaction = new FactionData // <-- Use FactionData
        {
            FactionName = msg.FactionName,
            FactionMembers = new List<string> { playerId },
            FactionLeaders = new List<string> { playerId }
        };

        _factionsComponent.FactionList.Add(newFaction);
        _invites.Remove(playerSession.UserId);
        SetMembership(playerSession, newFaction);
        Dirty(_factionsEntity.Value, _factionsComponent);
        Log.Info($"Player {playerSession.Name} created faction '{msg.FactionName}'.");

        // Send confirmation message
        var confirmationMsg = $"Фракция «{msg.FactionName}» создана.";
        _chatManager.ChatMessageToOne(ChatChannel.Notifications, confirmationMsg, confirmationMsg, sourceEntity, false, playerSession.Channel);

        // Notify the client their status changed
        var statusChangeEvent = new PlayerFactionStatusChangedEvent(true, newFaction.FactionName);
        RaiseNetworkEvent(statusChangeEvent, playerSession.Channel); // Target the specific player
    }

    /// <summary>
    /// Handles a player's request to leave their current faction, updating faction membership and notifying the player.
    /// </summary>
    private void OnLeaveFactionRequest(LeaveFactionRequestEvent msg, EntitySessionEventArgs args)
    {
        if (!EnsureFactionsComponent())
        {
            return;
        }
        var sourceEntity = _factionsEntity ?? EntityUid.Invalid;
        if (_factionsComponent == null || _factionsEntity == null) return;

        var playerSession = args.SenderSession;
        var playerId = playerSession.UserId.ToString();

        if (!TryGetPlayerFaction(playerSession.UserId, out var faction))
        {
            // FIX: Correct arguments for ChatMessageToOne
            var errorMsg = "Вы не состоите во фракции.";
            _chatManager.ChatMessageToOne(ChatChannel.Notifications, errorMsg, errorMsg, sourceEntity, false, playerSession.Channel);
            return;
        }

        faction!.FactionMembers.Remove(playerId);
        faction.FactionLeaders.Remove(playerId);
        if (faction.FactionLeaders.Count == 0 && faction.FactionMembers.Count > 0)
            faction.FactionLeaders.Add(faction.FactionMembers[0]);
        SetMembership(playerSession, null);
        Log.Info($"Player {playerSession.Name} left faction '{faction.FactionName}'.");

        // FIX: Correct arguments for ChatMessageToOne
        var confirmationMsg = $"Вы покинули фракцию «{faction.FactionName}».";
        _chatManager.ChatMessageToOne(ChatChannel.Notifications, confirmationMsg, confirmationMsg, sourceEntity, false, playerSession.Channel);

        if (faction.FactionMembers.Count == 0)
        {
            _factionsComponent.FactionList.Remove(faction);
            Log.Info($"Faction '{faction.FactionName}' disbanded as it became empty.");
        }

        Dirty(_factionsEntity.Value, _factionsComponent);

        // Notify the client their status changed
        var statusChangeEvent = new PlayerFactionStatusChangedEvent(false, null);
        RaiseNetworkEvent(statusChangeEvent, playerSession.Channel); // Target the specific player
    }

    /// <summary>
    /// Handles a request for a player to invite another player to their faction, performing validation and sending appropriate notifications and network events.
    /// </summary>
    private void OnInviteFactionRequest(InviteFactionRequestEvent msg, EntitySessionEventArgs args)
    {
        if (!EnsureFactionsComponent())
        {
            return;
        }
        var sourceEntity = _factionsEntity ?? EntityUid.Invalid;
        if (_factionsComponent == null || _factionsEntity == null) return;

        var inviterSession = args.SenderSession;
        var inviterId = inviterSession.UserId;

        if (!TryGetPlayerFaction(inviterId, out var inviterFaction))
        {
            // FIX: Correct arguments for ChatMessageToOne
            var errorMsg = "Чтобы приглашать игроков, вступите во фракцию.";
            _chatManager.ChatMessageToOne(ChatChannel.Notifications, errorMsg, errorMsg, sourceEntity, false, inviterSession.Channel);
            return;
        }

        if (!inviterFaction!.FactionLeaders.Contains(inviterId.ToString()))
        {
            Notify(inviterSession, "Приглашать игроков может только глава фракции.");
            return;
        }

        if (!_playerManager.TryGetSessionById(msg.TargetPlayerUserId, out var targetSession))
        {
            // FIX: Correct arguments for ChatMessageToOne
            var errorMsg = "Не удалось найти игрока, которого вы хотите пригласить.";
            _chatManager.ChatMessageToOne(ChatChannel.Notifications, errorMsg, errorMsg, sourceEntity, false, inviterSession.Channel);
            return;
        }

        if (IsPlayerInFaction(msg.TargetPlayerUserId, out _))
        {
            // FIX: Correct arguments for ChatMessageToOne (to inviter)
            var inviterErrorMsg = $"Игрок {targetSession.Name} уже состоит во фракции.";
            _chatManager.ChatMessageToOne(ChatChannel.Notifications, inviterErrorMsg, inviterErrorMsg, sourceEntity, false, inviterSession.Channel);

            // FIX: Correct arguments for ChatMessageToOne (to target)
            var targetErrorMsg = $"{inviterSession.Name} пригласил вас во фракцию «{inviterFaction!.FactionName}», но вы уже состоите во фракции.";
            _chatManager.ChatMessageToOne(ChatChannel.Notifications, targetErrorMsg, targetErrorMsg, sourceEntity, false, targetSession.Channel);
            return;
        }

        _invites[msg.TargetPlayerUserId] = (inviterFaction!, _timing.CurTime + TimeSpan.FromMinutes(5));
        var offerEvent = new FactionInviteOfferEvent(inviterSession.Name, inviterFaction!.FactionName, inviterId);
        RaiseNetworkEvent(offerEvent, Filter.SinglePlayer(targetSession));

        // FIX: Correct arguments for ChatMessageToOne (confirmation to inviter)
        var inviterConfirmMsg = $"Приглашение игроку {targetSession.Name} отправлено.";
        _chatManager.ChatMessageToOne(ChatChannel.Notifications, inviterConfirmMsg, inviterConfirmMsg, sourceEntity, false, inviterSession.Channel);

        // FIX: Correct arguments for ChatMessageToOne (notification to target)
        var targetNotifyMsg = $"{inviterSession.Name} приглашает вас во фракцию «{inviterFaction.FactionName}». Проверьте чат или уведомления.";
        _chatManager.ChatMessageToOne(ChatChannel.Notifications, targetNotifyMsg, targetNotifyMsg, sourceEntity, false, targetSession.Channel);

        Log.Info($"Player {inviterSession.Name} invited {targetSession.Name} to faction '{inviterFaction.FactionName}'.");
    }

    /// <summary>
    /// Handles a player's acceptance of a faction invitation, adding them to the specified faction and notifying them of the status change.
    /// </summary>
    private void OnAcceptFactionInvite(AcceptFactionInviteEvent msg, EntitySessionEventArgs args)
    {
        if (!EnsureFactionsComponent())
        {
            return;
        }
        var sourceEntity = _factionsEntity ?? EntityUid.Invalid;
        if (_factionsComponent == null || _factionsEntity == null) return;

        var accepterSession = args.SenderSession;
        var accepterId = accepterSession.UserId;
        var accepterIdStr = accepterId.ToString();

        if (IsPlayerInFaction(accepterId, out var currentFaction))
        {
            // FIX: Correct arguments for ChatMessageToOne
            var errorMsg = $"Вы уже состоите во фракции «{currentFaction!.FactionName}» и не можете принять приглашение.";
            _chatManager.ChatMessageToOne(ChatChannel.Notifications, errorMsg, errorMsg, sourceEntity, false, accepterSession.Channel);
            return;
        }

        var targetFaction = _factionsComponent.FactionList.FirstOrDefault(f => f.FactionName.Equals(msg.FactionName, StringComparison.OrdinalIgnoreCase));
        if (targetFaction == null)
        {
            // FIX: Correct arguments for ChatMessageToOne
            var errorMsg = $"Фракция «{msg.FactionName}» больше не существует.";
            _chatManager.ChatMessageToOne(ChatChannel.Notifications, errorMsg, errorMsg, sourceEntity, false, accepterSession.Channel);
            return;
        }

        if (!_invites.TryGetValue(accepterId, out var invite) || invite.Faction != targetFaction || invite.Expires <= _timing.CurTime)
        {
            Notify(accepterSession, "У вас нет действующего приглашения в эту фракцию. Попросите главу пригласить вас снова.");
            return;
        }
        _invites.Remove(accepterId);
        targetFaction.FactionMembers.Add(accepterIdStr);
        SetMembership(accepterSession, targetFaction);
        Dirty(_factionsEntity.Value, _factionsComponent);

        // FIX: Correct arguments for ChatMessageToOne
        var confirmationMsg = $"Вы вступили во фракцию «{targetFaction.FactionName}».";
        _chatManager.ChatMessageToOne(ChatChannel.Notifications, confirmationMsg, confirmationMsg, sourceEntity, false, accepterSession.Channel);
        Log.Info($"Player {accepterSession.Name} accepted invite and joined faction '{targetFaction.FactionName}'.");

        // Notify the client their status changed
        var statusChangeEvent = new PlayerFactionStatusChangedEvent(true, targetFaction.FactionName);
        RaiseNetworkEvent(statusChangeEvent, accepterSession.Channel); // Target the specific player
    }


    /// <summary>
    /// Determines whether the specified player is a member of any faction.
    /// </summary>
    /// <param name="userId">The user ID of the player to check.</param>
    /// <param name="faction">
    /// When this method returns, contains the faction the player belongs to if found; otherwise, null.
    /// </param>
    /// <returns>True if the player is in a faction; otherwise, false.</returns>

    private void Notify(ICommonSession session, string message)
    {
        _chatManager.ChatMessageToOne(ChatChannel.Notifications, message, message,
            _factionsEntity ?? EntityUid.Invalid, false, session.Channel);
    }

    private void SetMembership(ICommonSession session, FactionData? faction)
    {
        if (session.AttachedEntity is { } player)
        {
            var component = EnsureComp<CivFactionComponent>(player);
            component.SetFaction(faction?.FactionName ?? "");
            Dirty(player, component);
        }
    }

    private void OnSpawnComplete(PlayerSpawnCompleteEvent ev)
    {
        if (!EnsureFactionsComponent())
            return;
        TryGetPlayerFaction(ev.Player.UserId, out var faction);
        SetMembership(ev.Player, faction);
    }

    private void OnManageMember(ManageFactionMemberEvent msg, EntitySessionEventArgs args)
    {
        if (!EnsureFactionsComponent() || _factionsComponent == null || _factionsEntity == null)
            return;
        var session = args.SenderSession;
        if (!TryGetPlayerFaction(session.UserId, out var faction) || faction == null ||
            !faction.FactionLeaders.Contains(session.UserId.ToString()))
        {
            Notify(session, "Управлять участниками может только глава фракции.");
            return;
        }
        var target = msg.Target.ToString();
        if (msg.Target == session.UserId || !faction.FactionMembers.Contains(target))
        {
            Notify(session, "Выберите другого участника своей фракции.");
            return;
        }
        if (msg.TransferLeadership)
        {
            faction.FactionLeaders.Clear();
            faction.FactionLeaders.Add(target);
            Notify(session, "Руководство фракцией передано.");
        }
        else
        {
            faction.FactionMembers.Remove(target);
            faction.FactionLeaders.Remove(target);
            _invites.Remove(msg.Target);
            if (_playerManager.TryGetSessionById(msg.Target, out var removed))
            {
                SetMembership(removed, null);
                RaiseNetworkEvent(new PlayerFactionStatusChangedEvent(false, null), removed.Channel);
                Notify(removed, $"Вас исключили из фракции «{faction.FactionName}».");
            }
            Notify(session, "Игрок исключён из фракции.");
        }
        Dirty(_factionsEntity.Value, _factionsComponent);
        RaiseNetworkEvent(new PlayerFactionStatusChangedEvent(true, faction.FactionName), session.Channel);
    }

    public bool IsPlayerInFaction(NetUserId userId, out FactionData? faction) // <-- Use FactionData
    {
        faction = null;
        if (_factionsComponent == null)
            return false;

        var playerIdStr = userId.ToString();
        foreach (var f in _factionsComponent.FactionList)
        {
            if (f.FactionMembers.Contains(playerIdStr))
            {
                faction = f;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Attempts to find the faction that the specified player belongs to.
    /// </summary>
    /// <param name="userId">The user ID of the player.</param>
    /// <param name="faction">When this method returns, contains the player's faction if found; otherwise, null.</param>
    /// <returns>True if the player is a member of a faction; otherwise, false.</returns>
    public bool TryGetPlayerFaction(NetUserId userId, out FactionData? faction) // <-- Use FactionData
    {
        return IsPlayerInFaction(userId, out faction);
    }
}
