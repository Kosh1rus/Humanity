using System.Numerics;
using Content.Client.Humanity.Combat;
using Content.Shared.Humanity.Visuals;
using Content.Shared.Civ14.CivResearch;
using Content.Shared.Projectiles;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Client.Humanity.Visuals;

public sealed class HumanityAtmosphereSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly TransformSystem _transforms = default!;
    [Dependency] private readonly IOverlayManager _overlays = default!;
    [Dependency] private readonly PointLightSystem _lights = default!;
    private float _elapsed;
    private float _time;

    public override void Initialize()
    {
        base.Initialize();
        _overlays.AddOverlay(new HumanityAtmosphereOverlay());
        SubscribeAllEvent<ImpactEffectEvent>(OnImpact);
        SubscribeAllEvent<StoneDustEvent>(OnStoneDust);
    }

    public override void Shutdown()
    {
        _overlays.RemoveOverlay<HumanityAtmosphereOverlay>();
        base.Shutdown();
    }

    private void OnStoneDust(StoneDustEvent ev)
    {
        var coordinates = GetCoordinates(ev.Coordinates);
        if (!Deleted(coordinates.EntityId))
            Dust(_transforms.ToMapCoordinates(coordinates), 3);
    }

    private void OnImpact(ImpactEffectEvent ev)
    {
        if (ev.Prototype != "BulletImpactEffect")
            return;
        var coordinates = GetCoordinates(ev.Coordinates);
        if (Deleted(coordinates.EntityId) ||
            !HasComp<CivResearchComponent>(Transform(coordinates.EntityId).MapUid))
            return;
        Dust(_transforms.ToMapCoordinates(coordinates), 2);
    }

    private bool HasParticleBudget()
    {
        var count = 0;
        var query = EntityQueryEnumerator<BattleExplosionParticleComponent>();
        while (query.MoveNext(out _, out _))
        {
            if (++count >= 96)
                return false;
        }
        return true;
    }

    private void Dust(MapCoordinates coordinates, int count)
    {
        if (!HasParticleBudget())
            return;
        for (var i = 0; i < count; i++)
        {
            var uid = Spawn("HumanitySmallDust", coordinates);
            var particle = AddComp<BattleExplosionParticleComponent>(uid);
            particle.Velocity = _random.NextVector2(0.3f) + new Vector2(0, 0.15f);
            particle.Lifetime = _random.NextFloat(0.45f, 0.7f);
            particle.Growth = 0.16f;
            particle.Tint = new Color(0.67f, 0.62f, 0.52f, 0.42f);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        _elapsed += frameTime;
        _time += frameTime;
        if (_elapsed < 0.25f)
            return;
        _elapsed = 0;
        if (_players.LocalEntity is not { } player || !HasParticleBudget())
            return;
        var playerTransform = Transform(player);
        var playerPosition = _transforms.GetWorldPosition(playerTransform);
        var count = 0;
        var fires = EntityQueryEnumerator<FireAtmosphereComponent, PointLightComponent, TransformComponent>();
        while (fires.MoveNext(out var uid, out var fire, out var light, out var transform))
        {
            if (!light.Enabled || transform.MapUid != playerTransform.MapUid ||
                Vector2.DistanceSquared(_transforms.GetWorldPosition(transform), playerPosition) > 144f)
                continue;
            _lights.SetEnergy(uid, 3.2f + 0.18f * MathF.Sin(_time * 7.1f + uid.GetHashCode()), light);
            if (++count > 8 || !_random.Prob(0.35f))
                continue;
            var coordinates = _transforms.GetMapCoordinates(uid).Offset(new Vector2(0, 0.3f));
            var ember = Spawn("HumanityEmber", coordinates);
            var emberParticle = AddComp<BattleExplosionParticleComponent>(ember);
            emberParticle.Velocity = new Vector2(_random.NextFloat(-0.12f, 0.12f), 0.55f);
            emberParticle.Lifetime = 0.9f;
            emberParticle.Tint = Color.FromHex("#FFBC65CC");
            if (!fire.Smoke)
                continue;
            var smoke = Spawn("HumanitySmallDust", coordinates);
            var particle = AddComp<BattleExplosionParticleComponent>(smoke);
            particle.Velocity = new Vector2(0.12f, 0.35f);
            particle.Lifetime = 2.5f;
            particle.Growth = 0.12f;
            particle.Tint = new Color(0.57f, 0.56f, 0.53f, 0.24f);
        }
    }
}
