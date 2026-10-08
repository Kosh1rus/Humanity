using Content.Client.Construction;
using Robust.Client.Placement;
using Robust.Client.Placement.Modes;

namespace Content.Client.Humanity.Visuals;

public sealed partial class HumanityConstructionPreviewSystem : EntitySystem
{
    [Dependency] private IPlacementManager _placement = default!;

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);
        if (_placement is not PlacementManager { Hijack: ConstructionPlacementHijack hijack, CurrentMode: { } mode } manager)
            return;
        if (mode.GetType() == typeof(SnapgridCenter))
        {
            mode = new HumanityConstructionPlacementMode(manager)
            {
                MouseCoords = mode.MouseCoords,
                CurrentTile = mode.CurrentTile,
            };
            manager.CurrentMode = mode;
        }
        mode.ValidPlaceColor = mode is HumanityConstructionPlacementMode || hijack.CanPlacePreview(mode.MouseCoords)
            ? new Color(0.65f, 0.94f, 0.78f, 0.55f)
            : new Color(0.95f, 0.36f, 0.3f, 0.55f);
        mode.InvalidPlaceColor = new Color(0.95f, 0.36f, 0.3f, 0.55f);
    }
}
