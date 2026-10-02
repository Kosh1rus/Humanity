using System.Numerics;
using Content.Shared.Civ14.CivResearch;
using Content.Shared.Light.Components;
using Content.Client.Weather;
using Robust.Client.Graphics;
using Robust.Shared.Map;

namespace Content.Client.Humanity.Visuals;

public sealed class HumanityGroundOverlay(IEntityManager entities) : GridOverlay
{
    private readonly ITileDefinitionManager _tiles = IoCManager.Resolve<ITileDefinitionManager>();
    private readonly SharedMapSystem _map = entities.System<SharedMapSystem>();
    private readonly SharedTransformSystem _transforms = entities.System<SharedTransformSystem>();
    private readonly WeatherSystem _weather = entities.System<WeatherSystem>();
    private readonly Vector2[] _points = new Vector2[6];

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (!entities.TryGetComponent(Grid.Owner, out TransformComponent? transform) ||
            !entities.HasComponent<CivResearchComponent>(transform.MapUid))
            return;
        var handle = args.WorldHandle;
        entities.TryGetComponent(Grid.Owner, out RoofComponent? roof);
        handle.SetTransform(_transforms.GetWorldMatrix(Grid.Owner));
        var points = _points;
        foreach (var tile in _map.GetTilesIntersecting(Grid.Owner, Grid.Comp, args.WorldAABB))
        {
            var id = _tiles[tile.Tile.TypeId].ID;
            if (!(id.Contains("Dirt", StringComparison.Ordinal) || id.Contains("Grass", StringComparison.Ordinal)) ||
                !_weather.CanWeatherAffect(Grid.Owner, Grid.Comp, tile, roof))
                continue;
            var seed = unchecked((uint)(tile.GridIndices.X * 73856093 ^ tile.GridIndices.Y * 19349663));
            seed ^= seed >> 13;
            seed *= 1274126177u;
            if (seed % 7 != 0)
                continue;
            var offset = new Vector2(((seed >> 8) & 255) / 255f - 0.5f, ((seed >> 16) & 255) / 255f - 0.5f) * 0.3f;
            var center = (new Vector2(tile.GridIndices.X, tile.GridIndices.Y) + new Vector2(0.5f) + offset) * Grid.Comp.TileSize;
            var radius = (0.16f + (seed % 17) * 0.012f) * Grid.Comp.TileSize;
            for (var i = 0; i < 6; i++)
            {
                var angle = i * MathF.Tau / 6f + (seed % 19) * 0.17f;
                var jagged = 0.7f + ((seed >> (i * 3)) & 7) * 0.06f;
                points[i] = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle) * 0.6f) * radius * jagged;
            }
            handle.DrawPrimitives(DrawPrimitiveTopology.TriangleFan, points, new Color(0.18f, 0.14f, 0.095f, 0.35f));
            if ((seed & 3) == 0)
                handle.DrawLine(center + new Vector2(-radius * 0.3f, 0.03f), center + new Vector2(radius * 0.2f, 0.03f),
                    new Color(0.52f, 0.57f, 0.55f, 0.15f));
        }
        handle.SetTransform(Matrix3x2.Identity);
    }
}
