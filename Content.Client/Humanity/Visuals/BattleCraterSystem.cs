using System.Numerics;
using System.Linq;
using Content.Shared.Humanity.Visuals;
using Content.Shared.Standing;
using Robust.Client.GameObjects;

namespace Content.Client.Humanity.Visuals;

public sealed partial class BattleCraterSystem : EntitySystem
{
    [Dependency] private SharedTransformSystem _transforms = default!;
    private readonly Dictionary<EntityUid, float> _depths = new();
    private readonly List<(EntityUid Grid, Vector2 Position, float Radius)> _craters = new();

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);
        _craters.Clear();
        var craters = EntityQueryEnumerator<BattleScarComponent, TransformComponent, SpriteComponent>();
        while (craters.MoveNext(out _, out var scar, out var transform, out var sprite))
        {
            if (scar.Rubble || transform.GridUid is not { } grid)
                continue;
            sprite.Scale = new Vector2(scar.Radius * 2);
            _craters.Add((grid, _transforms.GetWorldPosition(transform), scar.Radius));
        }

        var bodies = EntityQueryEnumerator<StandingStateComponent, TransformComponent, SpriteComponent>();
        while (bodies.MoveNext(out var uid, out _, out var transform, out var sprite))
        {
            var position = _transforms.GetWorldPosition(transform);
            var target = 0f;
            foreach (var crater in _craters)
            {
                if (transform.GridUid != crater.Grid)
                    continue;
                var distance = (position - crater.Position) / crater.Radius;
                distance.Y *= 1.35f;
                if (distance.LengthSquared() < 0.64f)
                    target = 0.18f;
            }
            _depths.TryGetValue(uid, out var old);
            if (target == 0 && old == 0)
                continue;
            var depth = target;
            sprite.Offset += new Vector2(0, old - depth);
            if (depth == 0)
                _depths.Remove(uid);
            else
                _depths[uid] = depth;
        }
        foreach (var uid in _depths.Keys.Where(uid => Deleted(uid)).ToArray())
            _depths.Remove(uid);
    }

    public override void Shutdown()
    {
        foreach (var (uid, depth) in _depths)
        {
            if (TryComp<SpriteComponent>(uid, out var sprite))
                sprite.Offset += new Vector2(0, depth);
        }
        _depths.Clear();
        base.Shutdown();
    }
}
