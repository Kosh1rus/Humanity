using System.Numerics;
using Content.Shared.Civ14.CivResearch;
using Robust.Client.Graphics;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Client.Humanity.Visuals;

public sealed class NomadWaterOverlay(IEntityManager entities) : GridOverlay
{
    private readonly ITileDefinitionManager _tiles = IoCManager.Resolve<ITileDefinitionManager>();
    private readonly IGameTiming _timing = IoCManager.Resolve<IGameTiming>();
    private readonly SharedMapSystem _map = entities.System<SharedMapSystem>();
    private readonly SharedTransformSystem _transforms = entities.System<SharedTransformSystem>();
    private readonly NomadLandscapeSystem _landscape = entities.System<NomadLandscapeSystem>();

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (!entities.TryGetComponent(Grid.Owner, out TransformComponent? xform) ||
            !entities.TryGetComponent(xform.MapUid, out CivResearchComponent? research) || research.IsTDM)
            return;
        var handle = args.WorldHandle;
        handle.SetTransform(_transforms.GetWorldMatrix(Grid.Owner));
        var time = (float) (_timing.CurTime.TotalSeconds % 3600);
        foreach (var tile in _map.GetTilesIntersecting(Grid.Owner, Grid.Comp, args.WorldAABB))
        {
            if (!_landscape.IsWater(Grid.Owner, tile.GridIndices) &&
                !_tiles[tile.Tile.TypeId].ID.Contains("Water", StringComparison.OrdinalIgnoreCase))
                continue;
            var seed = unchecked((uint) (tile.GridIndices.X * 73856093 ^ tile.GridIndices.Y * 19349663));
            var phase = (seed % 1000) * 0.00628f;
            var center = ((Vector2) tile.GridIndices + new Vector2(0.5f)) * Grid.Comp.TileSize;
            var drift = MathF.Sin(time * 0.65f + phase);
            var color = new Color(0.63f, 0.79f, 0.81f, 0.06f + (drift + 1f) * 0.045f);
            var position = center + new Vector2(drift * 0.08f, MathF.Sin(time * 0.35f + phase) * 0.18f);
            handle.DrawLine(position - new Vector2(0.16f, 0), position + new Vector2(0.16f, 0.025f), color);
            if (seed % 3 == 0)
                handle.DrawLine(position + new Vector2(-0.09f, -0.16f), position + new Vector2(0.04f, -0.15f), color.WithAlpha(color.A * 0.5f));
        }
        handle.SetTransform(Matrix3x2.Identity);
    }
}
