using Content.Shared.Body;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;

namespace Content.Shared.Humanity.Body;

[RegisterComponent]
public sealed partial class LobotomyClumsyComponent : Component;

public sealed partial class LobotomyClumsySystem : EntitySystem
{
    [Dependency] private StatusEffectsSystem _effects = default!;
    private static readonly EntProtoId Effect = "HumanityLobotomyClumsy";

    [SubscribeLocalEvent]
    private void OnStartup(Entity<LobotomyClumsyComponent> ent, ref ComponentStartup args)
    {
        if (TryComp<OrganComponent>(ent, out var organ) && organ.Body is { } body)
            _effects.TrySetStatusEffectDuration(body, Effect);
    }

    [SubscribeLocalEvent]
    private void OnShutdown(Entity<LobotomyClumsyComponent> ent, ref ComponentShutdown args)
    {
        if (TryComp<OrganComponent>(ent, out var organ) && organ.Body is { } body)
            _effects.TryRemoveStatusEffect(body, Effect);
    }

    [SubscribeLocalEvent]
    private void OnInserted(Entity<LobotomyClumsyComponent> ent, ref OrganGotInsertedEvent args)
    {
        _effects.TrySetStatusEffectDuration(args.Target, Effect);
    }

    [SubscribeLocalEvent]
    private void OnRemoved(Entity<LobotomyClumsyComponent> ent, ref OrganGotRemovedEvent args)
    {
        _effects.TryRemoveStatusEffect(args.Target, Effect);
    }
}
