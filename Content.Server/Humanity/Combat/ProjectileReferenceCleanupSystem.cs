using Content.Shared.Projectiles;

namespace Content.Server.Humanity.Combat;

public sealed class ProjectileReferenceCleanupSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<EntityTerminatingEvent>(OnTerminating);
    }

    private void OnTerminating(ref EntityTerminatingEvent args)
    {
        var query = EntityQueryEnumerator<ProjectileComponent>();
        while (query.MoveNext(out var uid, out var projectile))
        {
            var changed = false;
            if (projectile.Shooter == args.Entity.Owner)
            {
                projectile.Shooter = null;
                changed = true;
            }
            if (projectile.Weapon == args.Entity.Owner)
            {
                projectile.Weapon = null;
                changed = true;
            }
            if (changed)
                Dirty(uid, projectile);
        }
    }
}
