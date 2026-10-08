using System.Numerics;
using Content.Shared._RMC14.Mortar;
using Robust.Client.Graphics;
using Robust.Shared.Enums;

namespace Content.Client.Humanity.Mortar;

public sealed partial class MortarAimPreviewSystem : EntitySystem
{
    [Dependency] private IOverlayManager _overlays = default!;
    public readonly HashSet<EntityUid> Aiming = new();

    public override void Initialize()
    {
        base.Initialize();
        _overlays.AddOverlay(new MortarAimOverlay(this, EntityManager) { ZIndex = 200 });
    }

    public override void Shutdown()
    {
        _overlays.RemoveOverlay<MortarAimOverlay>();
        Aiming.Clear();
        base.Shutdown();
    }
}

public sealed class MortarAimOverlay(MortarAimPreviewSystem preview, IEntityManager entities) : Overlay
{
    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    protected override void Draw(in OverlayDrawArgs args)
    {
        var transforms = entities.System<SharedTransformSystem>();
        foreach (var uid in preview.Aiming)
        {
            if (!entities.TryGetComponent<MortarComponent>(uid, out var mortar) || !mortar.Deployed ||
                !entities.TryGetComponent<TransformComponent>(uid, out var xform) ||
                !xform.Anchored || xform.MapID != args.MapId)
                continue;
            var start = transforms.GetWorldPosition(xform);
            var direction = SharedMortarSystem.GetAimDirection(mortar.Heading);
            var tip = start + direction * 1.8f;
            var side = new Vector2(-direction.Y, direction.X) * 0.18f;
            var color = new Color(0.9f, 0.8f, 0.5f, 0.85f);
            args.WorldHandle.DrawLine(start + direction * 0.55f, tip, color);
            args.WorldHandle.DrawLine(tip, tip - direction * 0.3f + side, color);
            args.WorldHandle.DrawLine(tip, tip - direction * 0.3f - side, color);
        }
    }
}
