using Content.Server.DoAfter;
using Content.Shared.TreeBranch;
using Content.Shared.DoAfter;
using Robust.Server.GameObjects;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Shared.Popups;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Verbs;
using Content.Shared.Examine;

namespace Content.Server.TreeBranch;

public sealed partial class TreeBranchesSystem : EntitySystem
{
    [Dependency] private IRobustRandom Random = default!;
    [Dependency] private IGameTiming _gameTiming = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private DoAfterSystem _doAfter = default!;
    [Dependency] private SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TreeBranchesComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<TreeBranchesComponent, CollectBranchDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<TreeBranchesComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
        SubscribeLocalEvent<TreeBranchesComponent, ExaminedEvent>(OnExamined);
    }

    private void OnMapInit(EntityUid uid, TreeBranchesComponent component, MapInitEvent args)
    {
        if (component.CurrentBranches < 0)
        {
            component.CurrentBranches = Random.Next(1, component.MaxBranches + 1);
            component.LastGrowthTime = _gameTiming.CurTime;
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<TreeBranchesComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (Paused(uid))
                continue;
            var currentTime = _gameTiming.CurTime;
            if (currentTime >= component.LastGrowthTime + TimeSpan.FromSeconds(component.GrowthTime))
            {
                if (component.CurrentBranches < component.MaxBranches)
                {
                    component.CurrentBranches++;
                    component.LastGrowthTime = currentTime;
                }
            }
        }
    }

private void OnGetVerbs(EntityUid uid, TreeBranchesComponent component, ref GetVerbsEvent<AlternativeVerb> args)
{
    if (!args.CanAccess || !args.CanInteract || component.CurrentBranches <= 0)
        return;

    var user = args.User;

    var verb = new AlternativeVerb
    {
        Text = "Собрать ветку",
        Act = () => StartCollectingBranch(uid, component, user)
    };
    args.Verbs.Add(verb);
}


    private void StartCollectingBranch(EntityUid treeUid, TreeBranchesComponent component, EntityUid user)
    {
        var doAfterArgs = new DoAfterArgs(EntityManager,
            user,
            component.CollectionTime,
            new CollectBranchDoAfterEvent(),
            treeUid)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = true
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnDoAfter(EntityUid uid, TreeBranchesComponent component, ref CollectBranchDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        args.Handled = true;
        if (component.CurrentBranches <= 0)
        {
            _popup.PopupEntity("Здесь больше нечего собирать.", uid, args.Args.User);
            return;
        }
        if (component.CurrentBranches == component.MaxBranches)
            component.LastGrowthTime = _gameTiming.CurTime;
        component.CurrentBranches--;
        var spawnPos = Transform(uid).MapPosition;
        var branch = Spawn("LeafedStick", spawnPos);
        _hands.TryPickupAnyHand(args.Args.User, branch);
        _popup.PopupEntity("Вы собрали ветку.", uid, args.Args.User);
        args.Handled = true;
    }

    private void OnExamined(EntityUid uid, TreeBranchesComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        var branchCount = component.CurrentBranches;
        if (branchCount > 0)
        {
            var branchWord = branchCount % 100 is >= 11 and <= 14 ? "веток" : (branchCount % 10) switch
            {
                1 => "ветка",
                >= 2 and <= 4 => "ветки",
                _ => "веток",
            };
            var message = $"Можно собрать ещё {branchCount} {branchWord}.";
            args.PushMarkup(message);
        }
    }

}
