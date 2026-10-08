using Content.Shared.Construction.Prototypes;
using Robust.Shared.Enums;
using Robust.Shared.Map;

namespace Content.Client.Construction;

public sealed partial class ConstructionPlacementHijack
{
    public bool CanPlacePreview(EntityCoordinates coordinates) => _prototype != null &&
        _constructionSystem.CanPlacePreview(_prototype, coordinates, Manager.Direction);
}

public sealed partial class ConstructionSystem
{
    public bool CanPlacePreview(ConstructionPrototype prototype, EntityCoordinates loc, Direction dir)
    {
        return loc.IsValid(EntityManager) && _playerManager.LocalEntity is { } user &&
            !GhostPresent(loc) && CheckConstructionConditions(prototype, loc, dir, user) &&
            _examineSystem.InRangeUnOccluded(user, loc, 20f,
                predicate: GetPredicate(prototype.CanBuildInImpassable, TransformSystem.ToMapCoordinates(loc)));
    }
}
