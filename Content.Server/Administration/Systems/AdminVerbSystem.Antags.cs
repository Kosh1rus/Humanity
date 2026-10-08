using Content.Server.GameTicking;
using Content.Server.Zombies;
using Content.Shared.Administration;
using Content.Server.Clothing.Systems;
using Content.Shared.Antag;
using Content.Shared.Database;
using Content.Shared.GameTicking.Rules.Components;
using Content.Shared.Humanoid;
using Content.Shared.Mind.Components;
using Content.Shared.Roles;
using Content.Shared.Verbs;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Content.Shared.Roles.Components;

namespace Content.Server.Administration.Systems;

public sealed partial class AdminVerbSystem
{

    // All antag verbs have names so invokeverb works.
    private void AddAntagVerbs(GetVerbsEvent<Verb> args)
    {
        if (!TryComp<ActorComponent>(args.User, out var actor))
            return;

        var player = actor.PlayerSession;

        if (!_adminManager.HasAdminFlag(player, AdminFlags.Fun))
            return;

        if (!HasComp<MindContainerComponent>(args.Target) || !TryComp<ActorComponent>(args.Target, out var targetActor))
            return;

        var targetPlayer = targetActor.PlayerSession;


    }
}
