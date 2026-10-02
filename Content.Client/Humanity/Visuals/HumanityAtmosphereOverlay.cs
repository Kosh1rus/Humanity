using Content.Shared.Civ14.CivResearch;
using System.Numerics;
using Content.Shared.Light.Components;
using Content.Client.Weather;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;
using Robust.Shared.Graphics;

namespace Content.Client.Humanity.Visuals;

public sealed class HumanityAtmosphereOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entities = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly IClyde _clyde = default!;
    [Dependency] private readonly IMapManager _maps = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    private readonly ShaderInstance _shader;
    private readonly SharedMapSystem _map;
    private readonly SharedTransformSystem _transforms;
    private readonly WeatherSystem _weather;
    private List<Entity<MapGridComponent>> _grids = new();
    private IRenderTexture? _roofMask;
    private bool _worldWar;
    private readonly List<FogBlast> _blasts = new();
    private readonly Vector2[] _blastPositions = new Vector2[8];
    private readonly float[] _blastRadii = new float[8];
    private readonly float[] _blastAges = new float[8];
    private readonly record struct FogBlast(MapCoordinates Coordinates, float Radius, TimeSpan Started);
    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;
    public override bool RequestScreenTexture => true;

    public HumanityAtmosphereOverlay()
    {
        IoCManager.InjectDependencies(this);
        _shader = _prototypes.Index<ShaderPrototype>("HumanityAtmosphere").InstanceUnique();
        _map = _entities.System<SharedMapSystem>();
        _transforms = _entities.System<SharedTransformSystem>();
        _weather = _entities.System<WeatherSystem>();
        ZIndex = 100;
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (!_entities.TryGetComponent(_maps.GetMapEntityId(args.MapId), out CivResearchComponent? research))
            return false;
        _worldWar = research.IsTDM;
        return true;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;
        DrawRoofMask(args);
        _shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _shader.SetParameter("ROOF_MASK", _roofMask!.Texture);
        var origin = args.Viewport.LocalToWorld(new Vector2(0, args.Viewport.Size.Y)).Position;
        _shader.SetParameter("world_origin", origin);
        _shader.SetParameter("world_axis_x", args.Viewport.LocalToWorld((Vector2) args.Viewport.Size).Position - origin);
        _shader.SetParameter("world_axis_y", args.Viewport.LocalToWorld(Vector2.Zero).Position - origin);
        _shader.SetParameter("fog_time", (float) (_timing.RealTime.TotalSeconds % 36000));
        UpdateBlasts(args.MapId);
        _shader.SetParameter("blast_positions", _blastPositions);
        _shader.SetParameter("blast_radii", _blastRadii);
        _shader.SetParameter("blast_ages", _blastAges);
        _shader.SetParameter("fog_strength", _worldWar ? 0.22f : 0.18f);
        _shader.SetParameter("saturation", _worldWar ? 0.94f : 1.02f);
        _shader.SetParameter("warmth", _worldWar ? -0.008f : 0.008f);
        var handle = args.WorldHandle;
        handle.UseShader(_shader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }

    public void Disperse(MapCoordinates coordinates, float radius)
    {
        if (!_entities.HasComponent<CivResearchComponent>(_maps.GetMapEntityId(coordinates.MapId)))
            return;
        if (_blasts.Count >= _blastAges.Length)
            _blasts.RemoveAt(0);
        _blasts.Add(new FogBlast(coordinates, Math.Clamp(radius, 2f, 10f), _timing.RealTime));
    }

    private void UpdateBlasts(MapId map)
    {
        _blasts.RemoveAll(blast => (_timing.RealTime - blast.Started).TotalSeconds >= 8 ||
            !_maps.MapExists(blast.Coordinates.MapId));
        Array.Fill(_blastAges, -1f);
        var index = 0;
        foreach (var blast in _blasts)
        {
            if (blast.Coordinates.MapId != map)
                continue;
            _blastPositions[index] = blast.Coordinates.Position;
            _blastRadii[index] = blast.Radius;
            _blastAges[index++] = (float) (_timing.RealTime - blast.Started).TotalSeconds;
        }
    }

    private void DrawRoofMask(in OverlayDrawArgs args)
    {
        var maskSize = args.Viewport.Size;
        if (_roofMask?.Size != maskSize)
        {
            _roofMask?.Dispose();
            _roofMask = _clyde.CreateRenderTarget(maskSize,
                new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba8Srgb),
                new TextureSampleParameters { Filter = true }, name: "humanity-fog-roofs");
        }
        _grids.Clear();
        _maps.FindGridsIntersecting(args.MapId, args.WorldAABB, ref _grids);
        var handle = args.WorldHandle;
        var bounds = args.WorldAABB;
        var invMatrix = args.Viewport.GetWorldToLocalMatrix();
        handle.UseShader(null);
        handle.RenderInRenderTarget(_roofMask, () =>
        {
            foreach (var grid in _grids)
            {
                handle.SetTransform(Matrix3x2.Multiply(_transforms.GetWorldMatrix(grid.Owner), invMatrix));
                _entities.TryGetComponent(grid.Owner, out RoofComponent? roof);
                foreach (var tile in _map.GetTilesIntersecting(grid.Owner, grid, bounds))
                {
                    if (_weather.CanWeatherAffect(grid.Owner, grid, tile, roof))
                        continue;
                    handle.DrawRect(new Box2(tile.GridIndices * grid.Comp.TileSize,
                        (tile.GridIndices + Vector2i.One) * grid.Comp.TileSize), Color.White);
                }
            }
        }, Color.Transparent);
        handle.SetTransform(Matrix3x2.Identity);
    }

    protected override void DisposeBehavior()
    {
        _shader.Dispose();
        _roofMask?.Dispose();
        base.DisposeBehavior();
    }
}
