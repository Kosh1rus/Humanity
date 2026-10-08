using Content.Shared._Shitmed.Targeting;
using Content.Shared.Body.Systems;
using Content.Shared.MedicalScanner;

namespace Content.Shared.Medical.HealthAnalyzer;

public abstract partial class HealthAnalyzerSystem
{
    [Dependency] private SharedBodySystem _humanityBody = default!;

    [SubscribeLocalEvent]
    private void OnHealthAnalyzerPartSelected(Entity<HealthAnalyzerComponent> analyzer, ref HealthAnalyzerPartMessage args)
    {
        if (analyzer.Comp.ScannedEntity is not { } target || GetEntity(args.Owner) != target)
            return;
        analyzer.Comp.CurrentBodyPart = null;
        if (args.BodyPart is { } selected)
        {
            var (type, symmetry) = _humanityBody.ConvertTargetBodyPart(selected);
            var parts = _humanityBody.GetBodyChildrenOfType(target, type, symmetry: symmetry);
            foreach (var part in parts)
            {
                analyzer.Comp.CurrentBodyPart = part.Id;
                break;
            }
        }
        UpdateUi(analyzer);
    }
}
