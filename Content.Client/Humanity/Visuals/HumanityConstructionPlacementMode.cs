using Content.Client.Construction;
using Robust.Client.Placement;
using Robust.Client.Placement.Modes;
using Robust.Shared.Map;

namespace Content.Client.Humanity.Visuals;

public sealed class HumanityConstructionPlacementMode(PlacementManager manager) : SnapgridCenter(manager)
{
    public override string ModeName => nameof(SnapgridCenter);

    public override bool IsValidPosition(EntityCoordinates position)
    {
        return base.IsValidPosition(position) &&
            pManager.Hijack is ConstructionPlacementHijack hijack && hijack.CanPlacePreview(position);
    }
}
