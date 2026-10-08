using System.Linq;
using System.Numerics;
using Content.Shared.Barricade;
using Content.Shared.Civ14.CivResearch;
using Content.Shared.Destructible;
using Content.Shared.Explosion.Components;
using Content.Shared.Humanity.Visuals;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;

namespace Content.Server.Humanity.Visuals;

public sealed partial class BattleLandscapeSystem : EntitySystem
{
    [Dependency] private SharedMapSystem _maps = default!;
    [Dependency] private SharedTransformSystem _transforms = default!;
    [Dependency] private IRobustRandom _random = default!;
    private readonly HashSet<EntityUid> _explosions = new();
    private readonly Dictionary<EntityUid, Queue<EntityUid>> _scars = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BarricadeComponent, DestructionEventArgs>(OnDestroyed);
    }

    private void OnDestroyed(EntityUid uid, BarricadeComponent component, DestructionEventArgs args)
    {
        CreateScar(_transforms.GetMapCoordinates(uid), 0.5f, true);
    }

    private void CreateScar(MapCoordinates origin, float radius, bool rubble)
    {
        if (!_maps.MapExists(origin.MapId))
            return;
        var map = _maps.GetMap(origin.MapId);
        if (!TryComp<CivResearchComponent>(map, out var research) || !research.IsTDM ||
            !_maps.TryFindGridAt(origin, out var grid, out _))
            return;
        var uid = Spawn(rubble ? null : "HumanityBattleCrater", new EntityCoordinates(grid,
            Vector2.Transform(origin.Position, _transforms.GetInvWorldMatrix(grid))));
        var scar = EnsureComp<BattleScarComponent>(uid);
        scar.Radius = radius;
        scar.Rubble = rubble;
        scar.Seed = _random.Next(1, int.MaxValue);
        Dirty(uid, scar);
        if (!rubble && TryComp<FixturesComponent>(uid, out var fixtures) &&
            fixtures.Fixtures.TryGetValue("crater", out var fixture))
            EntityManager.System<SharedPhysicsSystem>().SetRadius(uid, "crater", fixture, fixture.Shape, radius * 0.8f, fixtures);
        if (!_scars.TryGetValue(map, out var marks))
            _scars[map] = marks = new Queue<EntityUid>();
        marks.Enqueue(uid);
        while (marks.Count > 192)
        {
            var oldest = marks.Dequeue();
            if (!Deleted(oldest))
                QueueDel(oldest);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        _explosions.RemoveWhere(uid => Deleted(uid));
        var query = EntityQueryEnumerator<ExplosionVisualsComponent>();
        while (query.MoveNext(out var uid, out var explosion))
        {
            if (explosion.Intensity.Count == 0 || !_explosions.Add(uid) ||
                explosion.ExplosionType is not ("CivGrenade" or "CivDefault"))
                continue;
            CreateScar(explosion.Epicenter, Math.Clamp(explosion.Intensity.Count * 0.16f, 0.45f, 1.8f), false);
        }
        foreach (var map in _scars.Keys.ToArray())
        {
            if (Deleted(map))
                _scars.Remove(map);
        }
    }
}
