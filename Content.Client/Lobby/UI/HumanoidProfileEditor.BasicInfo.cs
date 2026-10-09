
using System.Linq;
using Content.Shared.Preferences;

namespace Content.Client.Lobby.UI;

public sealed partial class HumanoidProfileEditor
{
    private void SetName(string newName)
    {
        Profile = Profile?.WithName(newName);
        SetDirty();

        if (!IsDirty)
            return;

        SpriteView.SetName(newName);
    }

    private void UpdateNameEdit()
    {
        NameEdit.Text = Profile?.Name ?? "";
    }

    /// <summary>
    /// Randomize values selectively while respecting locked values.
    /// </summary>
    private void RandomizeProfile()
    {
        var config = HumanoidCharacterProfile.RandomizeConfigAll;
        foreach (var option in RandomizeOptions.Children.OfType<RandomizeLockButton>())
        {
            if (option.Locked)
                config &= ~option.For;
        }
        Profile = Profile == null
            ? HumanoidCharacterProfile.Random()
            : HumanoidCharacterProfile.Random(config, Profile);
        SetProfile(Profile, CharacterSlot);
        SetDirty();
    }
}
