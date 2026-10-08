using Content.Shared.Damage.Components;
using Content.Shared._Shitmed.Targeting;

namespace Content.Shared.Damage.Systems;

public sealed partial class DamageableSystem
{
    public DamageSpecifier? ChangeBodyDamage(EntityUid? uid, DamageSpecifier damage,
        bool ignoreResistances = false, bool interruptsDoAfters = true,
        DamageableComponent? damageable = null, EntityUid? origin = null,
        bool? canSever = true, bool? canEvade = false, float? partMultiplier = 1f,
        TargetBodyPart? targetPart = null, float armorPenetration = 0f, bool heavyAttack = false)
    {
        if (uid is not { } entity || !Resolve(entity, ref damageable, false))
            return null;
        return ChangeDamage((entity, damageable), damage, ignoreResistances, interruptsDoAfters,
            origin, canSever: canSever, canEvade: canEvade, partMultiplier: partMultiplier,
            targetPart: targetPart, armorPenetration: armorPenetration, heavyAttack: heavyAttack);
    }

    public void SetDamage(EntityUid uid, DamageableComponent damageable, DamageSpecifier damage)
        => SetDamage((uid, damageable), damage);

    public void DamageChanged(EntityUid uid, DamageableComponent damageable,
        DamageSpecifier? damageDelta = null, bool interruptsDoAfters = true,
        EntityUid? origin = null, bool? canSever = null)
        => OnEntityDamageChanged((uid, damageable), damageDelta, interruptsDoAfters, origin, canSever ?? true);
}
