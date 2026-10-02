using System.Linq;
using System.Numerics;
using Content.Shared._Shitmed.Targeting;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.Humanity.Visuals;

public static class HumanityTargetingLayout
{
    private static readonly Dictionary<TargetBodyPart, (Vector2 Position, Vector2 Size, string Name)> Regions = new()
    {
        [TargetBodyPart.Head] = (new(10, 3), new(9, 7), "Голова"),
        [TargetBodyPart.Torso] = (new(10, 10), new(9, 10), "Грудь"),
        [TargetBodyPart.Groin] = (new(10, 20), new(9, 4), "Пах"),
        [TargetBodyPart.RightArm] = (new(7, 11), new(4, 8), "Правая рука"),
        [TargetBodyPart.LeftArm] = (new(18, 11), new(4, 8), "Левая рука"),
        [TargetBodyPart.RightHand] = (new(7, 19), new(4, 3), "Правая кисть"),
        [TargetBodyPart.LeftHand] = (new(18, 19), new(4, 3), "Левая кисть"),
        [TargetBodyPart.RightLeg] = (new(10, 24), new(4, 5), "Правая нога"),
        [TargetBodyPart.LeftLeg] = (new(15, 24), new(4, 5), "Левая нога"),
        [TargetBodyPart.RightFoot] = (new(8, 29), new(6, 2), "Правая ступня"),
        [TargetBodyPart.LeftFoot] = (new(15, 29), new(6, 2), "Левая ступня"),
    };

    public static void Apply(Texture texture, Dictionary<TargetBodyPart, TextureButton> buttons)
    {
        foreach (var (part, button) in buttons)
        {
            var (position, size, name) = Regions[part];
            var scaledPosition = position * 3;
            var scaledSize = size * 3;
            button.SetSize = scaledSize;
            button.MinSize = scaledSize;
            LayoutContainer.SetMarginLeft(button, scaledPosition.X);
            LayoutContainer.SetMarginTop(button, scaledPosition.Y);
            LayoutContainer.SetMarginRight(button, scaledPosition.X + scaledSize.X);
            LayoutContainer.SetMarginBottom(button, scaledPosition.Y + scaledSize.Y);
            button.ToolTip = name;
            var overlay = (TextureRect) button.Children.First();
            overlay.SetSize = scaledSize;
            overlay.Texture = new AtlasTexture(texture, UIBox2.FromDimensions(position, size));
            overlay.ModulateSelfOverride = Color.FromHex("#FF7760");
            overlay.MouseFilter = Robust.Client.UserInterface.Control.MouseFilterMode.Ignore;
        }
    }
}
