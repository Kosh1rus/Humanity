using Content.Client.Humanity.Research;
using Content.Shared.Civ14.CivResearch;
using Content.Shared.Construction.Prototypes;

namespace Content.Client.Construction.UI;

public sealed partial class ConstructionMenu
{
    public void SetResearchProgress(string text)
    {
        ResearchProgress.SetMessage(text);
        ResearchProgress.Visible = !string.IsNullOrEmpty(text);
    }
}

internal sealed partial class ConstructionMenuPresenter
{
    private NomadProgressSystem _research = default!;
    private int _lastAge = -1;
    private string _search = string.Empty;

    private void InitializeResearchProgress()
    {
        _research = _entManager.System<NomadProgressSystem>();
        _research.ProgressChanged += OnResearchProgress;
    }

    private void OnResearchProgress(string text, int age)
    {
        _constructionView.SetResearchProgress(text);
        if (_lastAge == age)
            return;
        _lastAge = age;
        if (_constructionView.IsOpen)
            OnViewPopulateRecipes(_constructionView, (_search, _selectedCategory));
    }

    private bool IsAvailableInCurrentEra(ConstructionPrototype recipe)
    {
        if (_playerManager.LocalEntity is not { } player ||
            !_entManager.TryGetComponent<TransformComponent>(player, out var transform) ||
            transform.MapUid is not { } map ||
            !_entManager.TryGetComponent<CivResearchComponent>(map, out var research))
            return false;
        if (research.IsTDM)
            return recipe.TDM;
        var age = research.GetCurrentAge();
        return age >= recipe.AgeMin && age <= recipe.AgeMax;
    }
}
