using System;
using UnityEngine;

namespace Clockwork.Bertis.Gameplay
{
    public enum EntityFamily
    {
        None = 0,
        Player = 1,
        Zombie = 2,
        Friend = 3,
        ConcreteGround = 10,
        WoodGround = 11,
        MetalGround = 12,
        GravelGround = 13,
        ConcreteWall = 20,
        WoodWall = 21,
        MetalWall = 22,
        Explosive = 30
    }

    public class Entity : MonoBehaviour
    {
        [field: SerializeField]
        public EntityFamily Family { get; private set; }

        [field: SerializeField]
        public bool HasDamage { get; private set; }

        [field: SerializeField]
        public float BaseDamage { get; private set; }

        [field: SerializeField]
        public bool HasHealth { get; private set; }

        [field: SerializeField]
        public float BaseHealth { get; private set; }

        [field: SerializeField]
        public float CurrentHealth { get; private set; }

        public bool IsDead => HasHealth && CurrentHealth <= 0f;

        public float HealthRatio => CurrentHealth / BaseHealth;

        public static event ImpactAction EntityTookDamage;

        public event ImpactAction TookDamage;

        public event Action Revived;

        public event Action<Entity> HealthChanged;

        public void TakeDamage(in ImpactContext context)
        {
            if (context.IsVictimHurt)
            {
                CurrentHealth -= context.DamageDealt;
                HealthChanged?.Invoke(this);
                if (context.IsFatal)
                {
                    OnTookFatalDamage(context);
                }
                else
                {
                    OnTookNonFatalDamage(context);
                }
                TookDamage?.Invoke(context);
                EntityTookDamage?.Invoke(context);
            }
        }

        public virtual void Revive()
        {
            if (HasHealth)
            {
                CurrentHealth = BaseHealth;
                HealthChanged?.Invoke(this);
                Revived?.Invoke();
            }
        }

        protected virtual void OnTookFatalDamage(in ImpactContext context)
        {
        }

        protected virtual void OnTookNonFatalDamage(in ImpactContext context)
        {
        }
    }
}