using System.Numerics;
using Content.Client.GameTicking.Managers;
using Content.Client.Weather;
using Content.Shared.CCVar;
using Content.Shared.Civ14.CivResearch;
using Content.Shared.Humanity.Visuals;
using Content.Shared.Light.Components;
using Content.Shared.Light.EntitySystems;
using Robust.Client.Player;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Client.Humanity.Visuals;

public sealed partial class NomadAmbienceSystem : EntitySystem
{
    [Dependency] private IPlayerManager _players = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedTransformSystem _transforms = default!;
    [Dependency] private SharedMapSystem _maps = default!;
    [Dependency] private WeatherSystem _weather = default!;
    [Dependency] private IConfigurationManager _config = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private ClientGameTicker _ticker = default!;
    [Dependency] private MetaDataSystem _metadata = default!;

    private readonly string[] _paths =
    [
        "/Audio/Humanity/Ambience/birds.ogg",
        "/Audio/Humanity/Ambience/crickets.ogg",
        "/Audio/Effects/Weather/wind_2_1.ogg",
        "/Audio/Ambience/Objects/flowing_water_open.ogg",
    ];
    private readonly EntityUid?[] _streams = new EntityUid?[4];
    private readonly float[] _gains = new float[4];
    private readonly float[] _targets = new float[4];
    private float _sample;
    private float _volume;
    private EntityUid? _activeMap;

    public override void Initialize()
    {
        base.Initialize();
        Subs.CVar(_config, CCVars.AmbienceVolume,
            value => _volume = SharedAudioSystem.GainToVolume(value), true);
    }

    public override void Shutdown()
    {
        Stop();
        base.Shutdown();
    }

    private void Stop()
    {
        for (var i = 0; i < _streams.Length; i++)
        {
            _audio.Stop(_streams[i]);
            _streams[i] = null;
            _gains[i] = 0;
            _targets[i] = 0;
        }
        _activeMap = null;
        _sample = 0;
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);
        if (!TryComp<TransformComponent>(_players.LocalEntity, out var player) ||
            !TryComp<CivResearchComponent>(player.MapUid, out var research) || research.IsTDM)
        {
            if (_activeMap != null)
                Stop();
            return;
        }
        if (_activeMap != player.MapUid)
        {
            Stop();
            _activeMap = player.MapUid;
        }
        _sample -= frameTime;
        if (_sample <= 0)
        {
            _sample = 0.5f;
            Sample(player);
        }
        var blend = 1f - MathF.Exp(-frameTime / 1.5f);
        for (var i = 0; i < _streams.Length; i++)
        {
            _gains[i] += (_targets[i] - _gains[i]) * blend;
            if (_streams[i] == null && _gains[i] > 0.001f)
                _streams[i] = _audio.PlayGlobal(new SoundPathSpecifier(_paths[i]), Filter.Local(), false,
                    AudioParams.Default.WithLoop(true).WithVolume(-60))?.Entity;
            if (_streams[i] != null)
                _audio.SetVolume(_streams[i], SharedAudioSystem.GainToVolume(MathF.Max(0.00001f, _gains[i])) + _volume);
        }
    }

    private void Sample(TransformComponent player)
    {
        var outdoor = 0.15f;
        if (player.GridUid is { } grid && TryComp<MapGridComponent>(grid, out var gridComp) &&
            _maps.TryGetTileRef(grid, gridComp, player.Coordinates, out var tile))
            outdoor = _weather.CanWeatherAffect((grid, gridComp, null), tile) ? 1f : 0.15f;
        var position = _transforms.GetWorldPosition(player);
        var trees = 0;
        var foliage = EntityQueryEnumerator<FoliageAtmosphereComponent, TransformComponent>();
        while (foliage.MoveNext(out _, out var plant, out var xform))
        {
            if (plant.ShedLeaves && xform.MapUid == player.MapUid &&
                Vector2.DistanceSquared(position, _transforms.GetWorldPosition(xform)) < 49f)
                trees++;
        }
        var forest = Math.Clamp(trees / 12f, 0f, 1f);
        var waterDistance = 64f;
        var water = EntityQueryEnumerator<NomadWaterSurfaceComponent, TransformComponent>();
        while (water.MoveNext(out _, out _, out var xform))
        {
            if (xform.MapUid == player.MapUid)
                waterDistance = MathF.Min(waterDistance,
                    Vector2.DistanceSquared(position, _transforms.GetWorldPosition(xform)));
        }
        var day = 1f;
        if (TryComp<LightCycleComponent>(player.MapUid, out var cycle) && cycle.Enabled)
        {
            var time = (float) (_timing.CurTime + cycle.Offset - _ticker.RoundStartTimeSpan -
                _metadata.GetPauseTime(player.MapUid!.Value)).TotalSeconds;
            var light = (float) SharedLightCycleSystem.CalculateLightLevel(cycle, time);
            day = Math.Clamp((light - cycle.MinLightLevel) /
                MathF.Max(0.01f, MathF.Min(cycle.MaxLightLevel, cycle.ClipLight) - cycle.MinLightLevel), 0f, 1f);
        }
        _targets[0] = outdoor * day * (0.045f + forest * 0.055f);
        _targets[1] = outdoor * (1f - day) * 0.065f;
        _targets[2] = outdoor * forest * 0.045f;
        _targets[3] = outdoor * Math.Clamp(1f - MathF.Sqrt(waterDistance) / 8f, 0f, 1f) * 0.1f;
    }
}
