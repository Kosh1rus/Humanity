using Robust.Shared.GameStates;

namespace Content.Shared.Civ14.CivResearch;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CivResearchComponent : Component
{
    [DataField("researchEnabled"), AutoNetworkedField]
    public bool ResearchEnabled { get; set; } = true;

    [DataField("researchLevel"), AutoNetworkedField]
    public float ResearchLevel { get; set; }

    [DataField("researchSpeed"), AutoNetworkedField]
    public float ResearchSpeed { get; set; } = 100f / (3600f * 20f);

    [DataField("maxResearch"), AutoNetworkedField]
    public float MaxResearch { get; set; } = 800;

    [DataField("isTDM"), AutoNetworkedField]
    public bool IsTDM { get; set; }

    [DataField, AutoNetworkedField]
    public float ManualResearch { get; set; }

    [DataField, AutoNetworkedField]
    public float ManualResearchLimit { get; set; } = 40;

    public int GetCurrentAge() => (int) MathF.Floor(ResearchLevel / 100f);

    public bool CanResearch => ResearchEnabled && !IsTDM && ResearchLevel < MaxResearch;

    public float Advance(float points, bool manual = false)
    {
        if (!CanResearch || !float.IsFinite(points) || points <= 0)
            return 0;

        var age = GetCurrentAge();
        if (manual)
            points = MathF.Min(points, MathF.Max(0, ManualResearchLimit - ManualResearch));

        points = MathF.Min(points, MaxResearch - ResearchLevel);
        if (manual)
            points = MathF.Min(points, (age + 1) * 100f - ResearchLevel);

        ResearchLevel += points;
        if (manual)
            ManualResearch += points;
        if (GetCurrentAge() != age)
            ManualResearch = 0;
        return points;
    }
}
