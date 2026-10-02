using Robust.Client.Graphics;
using Robust.Shared.Enums;

namespace Content.Client.Humanity.Combat;

public sealed class BattleExplosionWaveOverlay(IEntityManager entities) : Overlay
{
    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    protected override void Draw(in OverlayDrawArgs args)
    {
        var transforms = entities.System<SharedTransformSystem>();
        var query = entities.EntityQueryEnumerator<BattleExplosionWaveComponent, TransformComponent>();
        while (query.MoveNext(out _, out var wave, out var transform))
        {
            if (transform.MapID != args.MapId)
                continue;
            var progress = Math.Clamp(wave.Age / wave.Lifetime, 0, 1);
            var radius = 0.15f + wave.MaxRadius * Math.Min(progress * 2f, 1f);
            var position = transforms.GetWorldPosition(transform);
            var alpha = (1f - progress) * 0.7f;
            for (var i = 0; i < 3; i++)
                args.WorldHandle.DrawCircle(position, radius + i * 0.05f,
                    new Color(0.82f, 0.8f, 0.72f, alpha / (i + 1)), false);
        }
    }
}
