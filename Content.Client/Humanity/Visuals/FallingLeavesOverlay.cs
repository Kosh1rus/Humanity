using System.Numerics;
using Robust.Client.Graphics;
using Robust.Shared.Enums;

namespace Content.Client.Humanity.Visuals;

public sealed class FallingLeavesOverlay(FoliageAtmosphereSystem foliage) : Overlay
{
    private readonly Vector2[] _points = new Vector2[4];
    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    protected override void Draw(in OverlayDrawArgs args)
    {
        foreach (var leaf in foliage.Leaves)
        {
            if (leaf.Origin.MapId != args.MapId)
                continue;
            var progress = leaf.Age / leaf.Lifetime;
            var position = leaf.Origin.Position + new Vector2(
                leaf.Age * 0.1f + MathF.Sin(leaf.Age * 2.2f + leaf.Phase) * 0.12f,
                leaf.Height * (1f - progress));
            var angle = leaf.Phase + leaf.Age * 1.8f;
            var longAxis = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 0.045f;
            var shortAxis = new Vector2(-longAxis.Y, longAxis.X) * 0.45f;
            var points = _points;
            points[0] = position + longAxis;
            points[1] = position + shortAxis;
            points[2] = position - longAxis;
            points[3] = position - shortAxis;
            var alpha = Math.Clamp(leaf.Age * 2f, 0, 1) * Math.Clamp((leaf.Lifetime - leaf.Age) * 1.5f, 0, 1);
            args.WorldHandle.DrawPrimitives(DrawPrimitiveTopology.TriangleFan, points, leaf.Color.WithAlpha(alpha));
        }
    }
}
