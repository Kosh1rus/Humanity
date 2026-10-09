using System.Linq;
using Content.Server.Chat.Managers;
using Content.Shared._Shitmed.PartStatus.Events;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Chat;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Utility;

namespace Content.Server._Shitmed.PartStatus;

public sealed partial class PartStatusSystem : EntitySystem
{
    [Dependency] private IChatManager _chat = default!;
    [Dependency] private MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<GetPartStatusEvent>(OnGetPartStatus);
    }

    private void OnGetPartStatus(GetPartStatusEvent message, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } user
            || !TryComp<TargetingComponent>(user, out var targeting)
            || _mobState.IsIncapacitated(user))
            return;

        var text = new FormattedMessage();
        text.AddText(Loc.GetString("humanity-body-status-title"));

        foreach (var (part, integrity) in targeting.BodyStatus.OrderBy(entry => entry.Key))
        {
            text.PushNewline();
            var name = Loc.GetString($"humanity-body-part-{part.ToString().ToLowerInvariant()}");
            var status = integrity.ToString().ToLowerInvariant();
            text.AddText(Loc.GetString("humanity-body-status-line",
                ("part", name), ("status", Loc.GetString($"humanity-body-status-{status}"))));
        }

        var markup = text.ToMarkup();
        _chat.ChatMessageToOne(ChatChannel.Emotes, markup, markup, EntityUid.Invalid,
            false, args.SenderSession.Channel, recordReplay: false);
    }
}
