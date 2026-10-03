using System.Numerics;
using Robust.Client.Graphics;
using Robust.Shared.Enums;

namespace Content.Client.Humanity.Visuals;

public sealed class NomadLandscapeOverlay(NomadLandscapeSystem landscape) : Overlay
{
    private readonly Vector2[] _chip = new Vector2[4];
    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;
        foreach (var mark in landscape.Marks)
        {
            if (mark.Coordinates.MapId != args.MapId || !args.WorldAABB.Contains(mark.Coordinates.Position))
                continue;
            var position = mark.Coordinates.Position;
            var color = mark.Color.WithAlpha(mark.Color.A * (1f - mark.Age / mark.Lifetime));
            if (mark.Ripple)
            {
                handle.DrawCircle(position, 0.12f + mark.Age * 0.38f, color, false);
                handle.DrawCircle(position, 0.06f + mark.Age * 0.24f, color.WithAlpha(color.A * 0.5f), false);
            }
            else if (mark.Debris)
            {
                position += mark.Direction * mark.Age + new Vector2(0, -0.7f * mark.Age * mark.Age);
                var axis = mark.Direction.LengthSquared() > 0.001f ? Vector2.Normalize(mark.Direction) * 0.035f : new Vector2(0.035f, 0);
                var side = new Vector2(-axis.Y, axis.X) * 0.5f;
                _chip[0] = position - axis - side;
                _chip[1] = position + axis - side;
                _chip[2] = position + axis + side;
                _chip[3] = position - axis + side;
                handle.DrawPrimitives(DrawPrimitiveTopology.TriangleFan, _chip, color);
            }
            else
            {
                var side = new Vector2(-mark.Direction.Y, mark.Direction.X) * 0.09f;
                handle.DrawLine(position - side, position - side + mark.Direction * 0.12f, color);
                handle.DrawLine(position + side, position + side + mark.Direction * 0.12f, color);
            }
        }
    }
}
