using System.Linq;
using Content.Server.Lathe;
using Content.Shared.Civ14.CivResearch;
using Content.Shared.Humanity.Research;
using Content.Shared.Lathe;
using Content.Shared.Research.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server.Humanity.Research;

public sealed partial class NomadProductionSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _prototypes = default!;
    [Dependency] private LatheSystem _lathes = default!;
    private float _elapsed;
    private readonly Dictionary<EntityUid, int> _ages = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NomadEraLatheComponent, LatheGetRecipesEvent>(OnRecipes);
    }

    private void OnRecipes(EntityUid uid, NomadEraLatheComponent component, LatheGetRecipesEvent args)
    {
        if (args.GetUnavailable)
            return;
        if (!TryComp<CivResearchComponent>(Transform(uid).MapUid, out var research) || research.IsTDM)
            return;
        var age = research.GetCurrentAge();
        args.Recipes.RemoveWhere(id => _prototypes.Index(id).CivAgeMin > age);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        _elapsed += frameTime;
        if (_elapsed < 1f)
            return;
        _elapsed = 0;
        var maps = EntityQueryEnumerator<CivResearchComponent>();
        while (maps.MoveNext(out var map, out var research))
        {
            var age = research.GetCurrentAge();
            if (_ages.TryGetValue(map, out var previous) && previous == age)
                continue;
            _ages[map] = age;
            var lathes = EntityQueryEnumerator<NomadEraLatheComponent, LatheComponent>();
            while (lathes.MoveNext(out var uid, out _, out var lathe))
            {
                if (Transform(uid).MapUid == map)
                    _lathes.UpdateUserInterfaceState(uid, lathe);
            }
        }
        foreach (var map in _ages.Keys.ToArray())
        {
            if (Deleted(map))
                _ages.Remove(map);
        }
    }
}
