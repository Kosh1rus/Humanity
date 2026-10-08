using System.Numerics;
using Content.Shared.Civ14.CivResearch;
using Content.Shared.Humanity.Visuals;
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
    private readonly Vector2[] _leafPoints = new Vector2[4];

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (!entities.TryGetComponent(Grid.Owner, out TransformComponent? transform) ||
            !entities.TryGetComponent(transform.MapUid, out CivResearchComponent? research))
            return;
        var handle = args.WorldHandle;
        entities.TryGetComponent(Grid.Owner, out RoofComponent? roof);
        handle.SetTransform(_transforms.GetWorldMatrix(Grid.Owner));
        var points = _points;
        foreach (var tile in _map.GetTilesIntersecting(Grid.Owner, Grid.Comp, args.WorldAABB))
        {
            var id = _tiles[tile.Tile.TypeId].ID;
            if (!(id.Contains("Dirt", StringComparison.Ordinal) || id.Contains("Grass", StringComparison.Ordinal)) ||
                !_weather.CanWeatherAffect((Grid.Owner, Grid.Comp, roof), tile))
                continue;
            var seed = unchecked((uint)(tile.GridIndices.X * 73856093 ^ tile.GridIndices.Y * 19349663));
            seed ^= seed >> 13;
            seed *= 1274126177u;
            if (seed % 7 != 0)
            {
                if (seed % 13 == 0)
                {
                    var pebble = ((Vector2) tile.GridIndices + new Vector2(0.3f, 0.4f)) * Grid.Comp.TileSize;
                    handle.DrawCircle(pebble, 0.035f * Grid.Comp.TileSize, new Color(0.4f, 0.4f, 0.32f, 0.32f));
                    handle.DrawLine(pebble, pebble + new Vector2(0.04f, 0.015f), new Color(0.61f, 0.59f, 0.46f, 0.26f));
                }
                if (id.Contains("Grass", StringComparison.Ordinal) && seed % 11 == 0)
                {
                    var grass = ((Vector2) tile.GridIndices + new Vector2(0.65f, 0.3f)) * Grid.Comp.TileSize;
                    var straw = new Color(0.61f, 0.55f, 0.3f, 0.22f);
                    handle.DrawLine(grass, grass + new Vector2(-0.04f, 0.1f), straw);
                    handle.DrawLine(grass + new Vector2(0.035f, 0), grass + new Vector2(0.06f, 0.13f), straw);
                }
                continue;
            }
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
        if (research.IsTDM)
        {
            var scars = entities.EntityQueryEnumerator<BattleScarComponent, TransformComponent>();
            while (scars.MoveNext(out _, out var scar, out var scarTransform))
            {
                var world = _transforms.GetWorldPosition(scarTransform);
                if (scarTransform.GridUid != Grid.Owner || !args.WorldAABB.Enlarged(scar.Radius).Contains(world))
                    continue;
                var center = Vector2.Transform(world, _transforms.GetInvWorldMatrix(Grid.Owner));
                var seed = unchecked((uint) scar.Seed);
                if (!scar.Rubble)
                {
                    continue;
                }
                for (var debris = 0; debris < 12; debris++)
                {
                    seed ^= seed << 13;
                    seed ^= seed >> 17;
                    seed ^= seed << 5;
                    var angle = (seed & 1023) * MathF.Tau / 1024;
                    var distance = scar.Radius * (0.6f + ((seed >> 10) & 255) / 255f * 0.7f);
                    var spot = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * distance;
                    var size = 0.06f + (seed % 5) * 0.012f;
                    _leafPoints[0] = spot + new Vector2(-size, -size * 0.4f);
                    _leafPoints[1] = spot + new Vector2(-size * 0.7f, size * 0.5f);
                    _leafPoints[2] = spot + new Vector2(size * 0.8f, size * 0.25f);
                    _leafPoints[3] = spot + new Vector2(size, -size * 0.4f);
                    handle.DrawPrimitives(DrawPrimitiveTopology.TriangleFan, _leafPoints,
                        new Color(0.49f, 0.43f, 0.32f, 0.7f));
                }
            }
        }
        else
        {
            var foliage = entities.EntityQueryEnumerator<FoliageAtmosphereComponent, TransformComponent>();
            while (foliage.MoveNext(out _, out var plant, out var plantTransform))
            {
                var world = _transforms.GetWorldPosition(plantTransform);
                if (plantTransform.GridUid != Grid.Owner || !args.WorldAABB.Contains(world))
                    continue;
                var center = Vector2.Transform(world, _transforms.GetInvWorldMatrix(Grid.Owner));
                var radius = plant.ShedLeaves ? 0.24f : 0.15f;
                for (var i = 0; i < points.Length; i++)
                {
                    var angle = i * MathF.Tau / points.Length;
                    points[i] = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle) * 0.45f) * radius;
                }
                handle.DrawPrimitives(DrawPrimitiveTopology.TriangleFan, points, new Color(0.075f, 0.07f, 0.045f, 0.25f));
                if (!plant.ShedLeaves || !plant.Enabled)
                    continue;
                var seed = unchecked((uint) ((int) (center.X * 32) * 73856093 ^ (int) (center.Y * 32) * 19349663));
                for (var leaf = 0; leaf < 7; leaf++)
                {
                    seed ^= seed << 13;
                    seed ^= seed >> 17;
                    seed ^= seed << 5;
                    var angle = (seed & 1023) * MathF.Tau / 1024;
                    var distance = 0.25f + ((seed >> 10) & 255) / 255f * 0.85f;
                    var spot = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * distance;
                    var local = new EntityCoordinates(Grid.Owner, spot);
                    if (!_map.TryGetTileRef(Grid.Owner, Grid.Comp, local, out var leafTile) ||
                        !_tiles[leafTile.Tile.TypeId].ID.Contains("Grass", StringComparison.Ordinal))
                        continue;
                    var direction = new Vector2(MathF.Cos(angle + 0.9f), MathF.Sin(angle + 0.9f)) * 0.055f;
                    var side = new Vector2(-direction.Y, direction.X) * 0.45f;
                    _leafPoints[0] = spot - direction;
                    _leafPoints[1] = spot + side;
                    _leafPoints[2] = spot + direction;
                    _leafPoints[3] = spot - side;
                    var color = (seed & 1) == 0
                        ? new Color(0.43f, 0.34f, 0.16f, 0.48f)
                        : new Color(0.47f, 0.46f, 0.22f, 0.4f);
                    handle.DrawPrimitives(DrawPrimitiveTopology.TriangleFan, _leafPoints, color);
                }
            }
            var camps = entities.EntityQueryEnumerator<NomadCampFootprintComponent, TransformComponent>();
            while (camps.MoveNext(out var uid, out var camp, out var campTransform))
            {
                if (campTransform.GridUid != Grid.Owner || !campTransform.Anchored ||
                    !args.WorldAABB.Contains(_transforms.GetWorldPosition(campTransform)) ||
                    !_map.TryGetTileRef(Grid.Owner, Grid.Comp, campTransform.Coordinates, out var tile) ||
                    !_tiles[tile.Tile.TypeId].ID.Contains("Grass", StringComparison.Ordinal))
                    continue;
                var center = Vector2.Transform(_transforms.GetWorldPosition(campTransform), _transforms.GetInvWorldMatrix(Grid.Owner));
                for (var ring = 0; ring < 3; ring++)
                {
                    var radius = camp.Radius * (0.6f + camp.Wear * 0.65f) * (1f - ring * 0.2f);
                    for (var i = 0; i < points.Length; i++)
                    {
                        var angle = i * MathF.Tau / points.Length;
                        points[i] = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle) * 0.75f) * radius;
                    }
                    var alpha = 0.045f + camp.Wear * 0.085f;
                    var color = camp.Charred ? new Color(0.13f, 0.10f, 0.07f, alpha) : new Color(0.34f, 0.27f, 0.15f, alpha);
                    handle.DrawPrimitives(DrawPrimitiveTopology.TriangleFan, points, color);
                }
                if (camp.Charred)
                {
                    var ash = new Color(0.2f, 0.18f, 0.15f, 0.45f);
                    for (var i = 0; i < 5; i++)
                    {
                        var angle = i * 2.4f + uid.GetHashCode() % 17;
                        var point = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * camp.Radius * 0.55f;
                        handle.DrawLine(point, point + new Vector2(0.065f, 0.025f), ash);
                    }
                }
            }
        }
        handle.SetTransform(Matrix3x2.Identity);
    }
}
