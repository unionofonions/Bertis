using UnityEngine;

#nullable enable

namespace Clockwork.Bertis.Gameplay;

public delegate void ImpactAction(in ImpactContext context);

public readonly ref struct ImpactContext
{
    public readonly Entity Instigator;
    public readonly Entity Victim;
    public readonly Vector3 ContactPoint;
    public readonly Vector3 ContactNormal;
    public readonly Vector3 ContactDirection;
    public readonly float DamageDealt;
    public readonly bool IsVictimHurt;
    public readonly bool IsFatal;

    public ImpactContext(
        Entity instigator,
        Entity victim,
        Vector3 contactPoint,
        Vector3 contactNormal,
        Vector3 contactDirection)
    {
        Instigator = instigator;
        Victim = victim;
        ContactPoint = contactPoint;
        ContactNormal = contactNormal;
        ContactDirection = contactDirection;
        DamageDealt = GetDamageDealt(instigator, victim);
        IsVictimHurt = DamageDealt > 0f;
        IsFatal = IsVictimHurt && DamageDealt >= victim.CurrentHealth;
    }

    private static float GetDamageDealt(Entity instigator, Entity victim)
    {
        if (instigator == null || victim == null)
        {
            return 0f;
        }
        if (!instigator.HasDamage || !victim.HasHealth || victim.IsDead)
        {
            return 0f;
        }
        return instigator.BaseDamage;
    }
}