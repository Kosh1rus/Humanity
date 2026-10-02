using Content.Shared.Damage;
using Content.Shared.Humanity.Visuals;
using Robust.Shared.Player;

namespace Content.Server.Humanity.Visuals;

public sealed class StoneDustSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StoneDustComponent, DamageChangedEvent>(OnDamage);
    }

    private void OnDamage(EntityUid uid, StoneDustComponent component, DamageChangedEvent args)
    {
        if (!args.DamageIncreased)
            return;
        var coordinates = Transform(uid).Coordinates;
        RaiseNetworkEvent(new StoneDustEvent(GetNetCoordinates(coordinates)), Filter.Pvs(uid, entityManager: EntityManager));
    }
}
