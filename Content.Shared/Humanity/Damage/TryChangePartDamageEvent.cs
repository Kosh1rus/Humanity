using Content.Shared._Shitmed.Targeting;

namespace Content.Shared.Damage.Systems;

[ByRefEvent]
public record struct TryChangePartDamageEvent(
    DamageSpecifier Damage, EntityUid? Origin = null, TargetBodyPart? TargetPart = null,
    bool IgnoreResistances = false, float ArmorPenetration = 0f,
    bool CanSever = true, bool CanEvade = false, float PartMultiplier = 1f,
    bool Evaded = false, bool Cancelled = false);
