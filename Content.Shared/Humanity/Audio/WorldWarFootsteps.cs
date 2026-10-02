using Content.Shared.Civ14.CivResearch;
using Content.Shared.Maps;
using Robust.Shared.Audio;

namespace Content.Shared.Movement.Systems;

public abstract partial class SharedMoverController
{
    private static readonly SoundSpecifier MudSteps = new SoundCollectionSpecifier("HumanityMudSteps",
        AudioParams.Default.WithVolume(-3f).WithVariation(0.08f));
    private static readonly SoundSpecifier GrassSteps = new SoundCollectionSpecifier("HumanityGrassSteps",
        AudioParams.Default.WithVolume(-4f).WithVariation(0.08f));

    private bool IsWorldWarMap(TransformComponent transform) =>
        TryComp<CivResearchComponent>(transform.MapUid, out var research) && research.IsTDM;

    private SoundSpecifier? GetWorldWarTerrainSound(TransformComponent transform, ContentTileDefinition tile)
    {
        if (!IsWorldWarMap(transform))
            return null;
        if (tile.ID.Contains("Snow", StringComparison.Ordinal) || tile.ID.Contains("Rock", StringComparison.Ordinal))
            return null;
        if (tile.ID.Contains("Dirt", StringComparison.Ordinal))
            return MudSteps;
        return tile.ID.Contains("Grass", StringComparison.Ordinal) ? GrassSteps : null;
    }
}
