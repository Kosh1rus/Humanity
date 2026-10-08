using Content.Shared.Damage.Components;
using Content.Shared.Projectiles;
using Content.Shared.Standing;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Events;
using Robust.Shared.Random;
using Content.Shared.Mobs.Systems;

namespace Content.Shared.Damage.Systems;

public sealed partial class RequireProjectileTargetSystem : EntitySystem
{
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RequireProjectileTargetComponent, PreventCollideEvent>(PreventCollide);
        SubscribeLocalEvent<RequireProjectileTargetComponent, StoodEvent>(StandingBulletHit);
        SubscribeLocalEvent<RequireProjectileTargetComponent, DownedEvent>(LayingBulletPass);

    }

    private void PreventCollide(Entity<RequireProjectileTargetComponent> ent, ref PreventCollideEvent args)
    {
        if (args.Cancelled)
            return;

        if (!ent.Comp.Active)
            return;

        var other = args.OtherEntity;
        // Resolve the ProjectileComponent on the 'other' entity (the projectile)
        if (TryComp(other, out ProjectileComponent? projectileComp) &&
            CompOrNull<TargetedProjectileComponent>(other)?.Target != ent)
        {
            if (_mobState.IsDead(ent))
            {
                args.Cancelled = true;
                return;
            }

            if (_random.Prob(0.2f))
                return;

            // Prevents shooting out of while inside of crates
            var shooter = projectileComp.Shooter;
            if (!shooter.HasValue)
                return;

            // ProjectileGrenades delete the entity that's shooting the projectile,
            // so it's impossible to check if the entity is in a container
            if (TerminatingOrDeleted(shooter.Value))
            {
                // If the shooter is deleted, nullify the reference in the projectile component
                // to prevent network errors when serializing ProjectileComponent's state.
                // This ensures that GetNetEntity won't be called on a deleted entity.
                projectileComp.Shooter = null;
                Dirty(other, projectileComp); // Mark the ProjectileComponent as dirty so the change is networked.
                return;
            }

            if (!_container.IsEntityOrParentInContainer(shooter.Value))
                args.Cancelled = true;
        }
    }

    private void SetActive(Entity<RequireProjectileTargetComponent> ent, bool value)
    {
        if (ent.Comp.Active == value)
            return;

        ent.Comp.Active = value;
        Dirty(ent);
    }

    private void StandingBulletHit(Entity<RequireProjectileTargetComponent> ent, ref StoodEvent args)
    {
        SetActive(ent, false);
    }

    private void LayingBulletPass(Entity<RequireProjectileTargetComponent> ent, ref DownedEvent args)
    {
        SetActive(ent, true); // stalker-changes
    }
}
