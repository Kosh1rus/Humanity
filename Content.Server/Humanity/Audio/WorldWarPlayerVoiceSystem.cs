using System.Linq;
using Content.Shared.Civ14.CivResearch;
using Content.Shared.Damage;
using Content.Shared.Humanoid;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server.Humanity.Audio;

public sealed class WorldWarPlayerVoiceSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    private readonly Dictionary<EntityUid, TimeSpan> _nextCry = new();
    private TimeSpan _nextCleanup;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ActorComponent, DamageChangedEvent>(OnDamage);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (_nextCleanup > _timing.CurTime)
            return;
        _nextCleanup = _timing.CurTime + TimeSpan.FromSeconds(10);
        foreach (var uid in _nextCry.Where(pair => pair.Value <= _timing.CurTime).Select(pair => pair.Key).ToArray())
            _nextCry.Remove(uid);
    }

    private void OnDamage(EntityUid uid, ActorComponent component, DamageChangedEvent args)
    {
        if (!args.DamageIncreased || args.DamageDelta == null ||
            !TryComp<CivResearchComponent>(Transform(uid).MapUid, out var research) || !research.IsTDM ||
            !TryComp<HumanoidAppearanceComponent>(uid, out var appearance) ||
            !TryComp<MobStateComponent>(uid, out var state) || state.CurrentState != MobState.Alive ||
            _nextCry.TryGetValue(uid, out var next) && next > _timing.CurTime)
            return;
        var wound = 0f;
        foreach (var (type, damage) in args.DamageDelta.DamageDict)
        {
            if (type is "Piercing" or "Slash" or "Blunt" or "Heat" && damage > 0)
                wound += (float) damage;
        }
        if (wound < 15f)
            return;
        _nextCry[uid] = _timing.CurTime + TimeSpan.FromSeconds(10);
        TryPlayScream(uid);
    }

    public bool TryPlayScream(EntityUid uid)
    {
        if (!TryComp<CivResearchComponent>(Transform(uid).MapUid, out var research) || !research.IsTDM ||
            !TryComp<HumanoidAppearanceComponent>(uid, out var appearance))
            return false;
        var collection = appearance.Sex == Sex.Female ? "HumanityBattleFemaleScreams" : "HumanityBattleMaleScreams";
        _audio.PlayPvs(new SoundCollectionSpecifier(collection), uid,
            AudioParams.Default.WithVolume(-2f).WithVariation(0.06f).WithMaxDistance(14f));
        return true;
    }
}
