using Content.Shared.Body.Part;

namespace Content.Shared.Humanoid;

public static class BodyPartLayerExtensions
{
    public static HumanoidVisualLayers? ToHumanoidLayers(this BodyPartComponent part)
        {
            switch (part.PartType)
            {
                case BodyPartType.Other:
                    break;
                case BodyPartType.Torso:
                    return HumanoidVisualLayers.Chest;
                case BodyPartType.Tail:
                    return HumanoidVisualLayers.Tail;
                case BodyPartType.Head:
                    // use the Sublayers method to hide the rest of the parts,
                    // if that's what you're looking for
                    return HumanoidVisualLayers.Head;
                case BodyPartType.Arm:
                    switch (part.Symmetry)
                    {
                        case BodyPartSymmetry.None:
                            break;
                        case BodyPartSymmetry.Left:
                            return HumanoidVisualLayers.LArm;
                        case BodyPartSymmetry.Right:
                            return HumanoidVisualLayers.RArm;
                    }

                    break;
                case BodyPartType.Hand:
                    switch (part.Symmetry)
                    {
                        case BodyPartSymmetry.None:
                            break;
                        case BodyPartSymmetry.Left:
                            return HumanoidVisualLayers.LHand;
                        case BodyPartSymmetry.Right:
                            return HumanoidVisualLayers.RHand;
                    }

                    break;
                case BodyPartType.Leg:
                    switch (part.Symmetry)
                    {
                        case BodyPartSymmetry.None:
                            break;
                        case BodyPartSymmetry.Left:
                            return HumanoidVisualLayers.LLeg;
                        case BodyPartSymmetry.Right:
                            return HumanoidVisualLayers.RLeg;
                    }

                    break;
                case BodyPartType.Foot:
                    switch (part.Symmetry)
                    {
                        case BodyPartSymmetry.None:
                            break;
                        case BodyPartSymmetry.Left:
                            return HumanoidVisualLayers.LFoot;
                        case BodyPartSymmetry.Right:
                            return HumanoidVisualLayers.RFoot;
                    }

                    break;
            }

            return null;
        }
}
