using Content.Server.Chat.Systems;
using Content.Server.DoAfter;
using Content.Shared.Civ14.CivResearch;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Humanity.Research;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Content.Shared.Verbs;
using Robust.Shared.Map;
using Robust.Shared.Player;

namespace Content.Server.Humanity.Research;

public sealed partial class NomadResearchSystem : EntitySystem
{
    [Dependency] private SharedMapSystem _maps = default!;
    [Dependency] private DoAfterSystem _doAfter = default!;
    [Dependency] private SharedStackSystem _stacks = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private ChatSystem _chat = default!;
    private float _elapsed;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NomadResearchTableComponent, InteractUsingEvent>(OnInteract);
        SubscribeLocalEvent<NomadResearchTableComponent, NomadExperimentDoAfterEvent>(OnExperiment);
        SubscribeLocalEvent<NomadResearchTableComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<NomadResearchTableComponent, GetVerbsEvent<Verb>>(OnVerbs);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        _elapsed += frameTime;
        if (_elapsed < 1f)
            return;
        var elapsed = _elapsed;
        _elapsed = 0;
        var query = EntityQueryEnumerator<CivResearchComponent>();
        while (query.MoveNext(out var uid, out var research))
        {
            if (!research.CanResearch || Paused(uid))
                continue;
            var age = research.GetCurrentAge();
            if (research.Advance(research.ResearchSpeed * 20f * elapsed) <= 0)
                continue;
            Dirty(uid, research);
            AnnounceAge(uid, age, research);
        }
    }

    private bool TryResearch(EntityUid uid, out EntityUid map, out CivResearchComponent research)
    {
        map = _maps.GetMap(Transform(uid).MapID);
        return TryComp(map, out research!);
    }

    public bool TrySetAge(EntityUid map, int age)
    {
        if (!TryComp<CivResearchComponent>(map, out var research) || research.IsTDM ||
            age < 0 || age > 8 || age * 100f > research.MaxResearch)
            return false;

        var previousAge = research.GetCurrentAge();
        research.ResearchLevel = age * 100f;
        research.ManualResearch = 0;
        Dirty(map, research);
        AnnounceAge(map, previousAge, research);
        return true;
    }

    private void OnInteract(EntityUid uid, NomadResearchTableComponent table, InteractUsingEvent args)
    {
        if (args.Handled || !TryResearch(uid, out _, out var research))
            return;
        args.Handled = true;
        if (!research.CanResearch || research.ManualResearch >= research.ManualResearchLimit)
        {
            _popup.PopupEntity("Лимит опытов исчерпан. Дождитесь новой эпохи.", uid, args.User);
            return;
        }
        if (table.Busy)
        {
            _popup.PopupEntity("Стол занят.", uid, args.User);
            return;
        }
        var age = research.GetCurrentAge();
        var experiment = NomadResearch.Experiment(age);
        if (!TryComp<StackComponent>(args.Used, out var stack) || stack.StackTypeId != experiment.Stack || stack.Count < experiment.Count)
        {
            _popup.PopupEntity($"Нужно: {experiment.Name} ×{experiment.Count}. Используйте на столе.", uid, args.User);
            return;
        }
        var doAfter = new DoAfterArgs(EntityManager, args.User, table.ExperimentDuration,
            new NomadExperimentDoAfterEvent(GetNetEntity(args.Used), age), uid, target: uid, used: args.Used)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = true,
        };
        table.Busy = _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnExperiment(EntityUid uid, NomadResearchTableComponent table, ref NomadExperimentDoAfterEvent args)
    {
        table.Busy = false;
        if (args.Cancelled || args.Handled)
            return;
        args.Handled = true;
        if (!TryResearch(uid, out var map, out var research) || !research.CanResearch ||
            research.GetCurrentAge() != args.Age || research.ManualResearch >= research.ManualResearchLimit)
        {
            _popup.PopupEntity("Условия исследования изменились. Материалы сохранены.", uid, args.Args.User);
            return;
        }
        var material = GetEntity(args.Material);
        var experiment = NomadResearch.Experiment(args.Age);
        if (TerminatingOrDeleted(material) || EntityManager.IsQueuedForDeletion(material) || !_hands.IsHolding(args.Args.User, material) ||
            !TryComp<StackComponent>(material, out var stack) || stack.StackTypeId != experiment.Stack ||
            !_stacks.TryUse((material, stack), experiment.Count))
            return;

        var points = research.Advance(table.ResearchPoints, manual: true);
        Dirty(map, research);
        _popup.PopupEntity($"Опыт завершён: +{points:F0} к развитию.", uid, args.Args.User);
        AnnounceAge(map, args.Age, research);
    }

    private void OnExamine(EntityUid uid, NomadResearchTableComponent table, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange || !TryResearch(uid, out _, out var research))
            return;
        args.PushText(NomadResearch.Status(research));
        if (!research.CanResearch)
            return;
        var experiment = NomadResearch.Experiment(research.GetCurrentAge());
        args.PushText($"Опыт: {experiment.Name}, {experiment.Count} шт.; {table.ExperimentDuration:F0} сек.; до {table.ResearchPoints:F0} очков. Материалы расходуются только при успехе.");
        if (table.Busy)
            args.PushText("Стол занят исследованием.");
    }

    private void OnVerbs(EntityUid uid, NomadResearchTableComponent table, ref GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;
        var user = args.User;
        args.Verbs.Add(new Verb
        {
            Text = "Прогресс цивилизации",
            Act = () =>
            {
                if (TryResearch(uid, out _, out var research))
                    _popup.PopupEntity(NomadResearch.Status(research), uid, user, PopupType.Large);
            },
        });
    }

    private void AnnounceAge(EntityUid map, int previousAge, CivResearchComponent research)
    {
        if (previousAge == research.GetCurrentAge())
            return;
        _chat.DispatchFilteredAnnouncement(Filter.Empty().AddWhere(session =>
                session.AttachedEntity is { } player && Transform(player).MapUid == map),
            $"Новая эпоха: {NomadResearch.AgeName(research.GetCurrentAge())}.",
            map, "Развитие цивилизации", playSound: false);
    }
}
