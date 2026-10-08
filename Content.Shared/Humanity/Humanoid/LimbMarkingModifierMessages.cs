using Content.Shared.Humanoid.Markings;
using Robust.Shared.Serialization;

namespace Content.Shared.Humanoid;

[Serializable, NetSerializable]
public sealed class LimbHumanoidMarkingModifierMarkingSetMessage : BoundUserInterfaceMessage
{
    public MarkingSet MarkingSet { get; }
    public bool ResendState { get; }

    public LimbHumanoidMarkingModifierMarkingSetMessage(MarkingSet set, bool resendState)
    {
        MarkingSet = set;
        ResendState = resendState;
    }
}

[Serializable, NetSerializable]
public sealed class LimbHumanoidMarkingModifierBaseLayersSetMessage : BoundUserInterfaceMessage
{
    public LimbHumanoidMarkingModifierBaseLayersSetMessage(HumanoidVisualLayers layer, CustomBaseLayerInfo? info, bool resendState)
    {
        Layer = layer;
        Info = info;
        ResendState = resendState;
    }

    public HumanoidVisualLayers Layer { get; }
    public CustomBaseLayerInfo? Info { get; }
    public bool ResendState { get; }
}

[Serializable, NetSerializable]
public sealed class LimbHumanoidMarkingModifierState : BoundUserInterfaceState
{
    // TODO just use the component state, remove the BUI state altogether.
    public LimbHumanoidMarkingModifierState(
        MarkingSet markingSet,
        string species,
        Sex sex,
        Color skinColor,
        Dictionary<HumanoidVisualLayers, CustomBaseLayerInfo> customBaseLayers
    )
    {
        MarkingSet = markingSet;
        Species = species;
        Sex = sex;
        SkinColor = skinColor;
        CustomBaseLayers = customBaseLayers;
    }

    public MarkingSet MarkingSet { get; }
    public string Species { get; }
    public Sex Sex { get; }
    public Color SkinColor { get; }
    public Color EyeColor { get; }
    public Color? HairColor { get; }
    public Color? FacialHairColor { get; }
    public Dictionary<HumanoidVisualLayers, CustomBaseLayerInfo> CustomBaseLayers { get; }
}
