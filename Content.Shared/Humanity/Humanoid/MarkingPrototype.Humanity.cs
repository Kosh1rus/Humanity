namespace Content.Shared.Humanoid.Markings;

public sealed partial class MarkingPrototype
{
    [DataField("markingCategory")] public MarkingCategories MarkingCategory { get; private set; } = MarkingCategories.Chest;
    [DataField("speciesRestriction")] public List<string>? SpeciesRestrictions { get; private set; }
}
