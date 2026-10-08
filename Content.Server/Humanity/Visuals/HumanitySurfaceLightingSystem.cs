using Content.Shared.Civ14.CivResearch;
using Content.Shared.Light.Components;
using Robust.Shared.Map.Components;

namespace Content.Server.Humanity.Visuals;

public sealed class HumanitySurfaceLightingSystem : EntitySystem
{
    private float _elapsed;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        _elapsed += frameTime;
        if (_elapsed < 1f)
            return;
        _elapsed = 0f;

        var grids = EntityQueryEnumerator<MapGridComponent, ImplicitRoofComponent, TransformComponent>();
        while (grids.MoveNext(out var uid, out _, out _, out var transform))
        {
            if (!HasComp<CivResearchComponent>(transform.MapUid))
                continue;

            EnsureComp<RoofComponent>(uid);
            RemCompDeferred<ImplicitRoofComponent>(uid);
        }
    }
}
