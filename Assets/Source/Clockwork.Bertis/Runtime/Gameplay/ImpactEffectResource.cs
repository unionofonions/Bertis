using System;
using UnityEngine;
using Clockwork.Collections;
using Clockwork.Pooling;
using Clockwork.Simulation;

#nullable enable

namespace Clockwork.Bertis.Gameplay
{
    public enum ImpactEffectOrientation
    {
        Incident,
        InverseIncident,
        Normal,
        Reflection
    }

    [Serializable]
    public class ImpactEffectData
    {
        [SerializeField]
        [PrefabReference]
        private ParticleDescriptor? _hitParticle;

        [SerializeField]
        private ImpactEffectOrientation _hitParticleOrientation;

        [SerializeField]
        [PrefabReference]
        private DecalDescriptor? _hitDecal;

        [SerializeField]
        private SoundDescriptor? _hitSound;

        [SerializeField]
        [PrefabReference]
        private ParticleDescriptor? _killParticle;

        [SerializeField]
        private ImpactEffectOrientation _killParticleOrientation;

        [SerializeField]
        private SoundDescriptor? _killSound;

        public void PlayEffects(in ImpactContext context)
        {
            if (!context.IsFatal)
            {
                if (PrefabRegistry.Rent(_hitParticle, out ParticleDescriptor? hitParticle))
                {
                    Quaternion rotation = ToRotation(_hitParticleOrientation, context.ContactDirection, context.ContactNormal);
                    hitParticle.Play(context.ContactPoint, rotation);
                }
                if (PrefabRegistry.Rent(_hitDecal, out DecalDescriptor? hitDecal))
                {
                    hitDecal.PlaceNormal(context.ContactPoint, context.ContactNormal);
                }
                SoundOutputModel.Play(_hitSound, context.ContactPoint);
            }
            else
            {
                if (PrefabRegistry.Rent(_killParticle, out ParticleDescriptor? killParticle))
                {
                    Quaternion rotation = ToRotation(_killParticleOrientation, context.ContactDirection, context.ContactNormal);
                    killParticle.Play(context.ContactPoint, rotation);
                }
                SoundOutputModel.Play(_killSound, context.ContactPoint);
            }
        }

        protected static Quaternion ToRotation(ImpactEffectOrientation orientation, Vector3 direction, Vector3 normal)
        {
            return orientation switch
            {
                ImpactEffectOrientation.Incident => Quaternion.LookRotation(direction),
                ImpactEffectOrientation.InverseIncident => Quaternion.LookRotation(-direction),
                ImpactEffectOrientation.Normal => Quaternion.LookRotation(normal),
                ImpactEffectOrientation.Reflection => Quaternion.LookRotation(Vector3.Reflect(direction, normal)),
                _ => Quaternion.identity
            };
        }
    }

    [CreateAssetMenu(menuName = "Clockwork/Bertis/Gameplay/Impact Effect Resource")]
    public class ImpactEffectResource : ScriptableObject
    {
        [SerializeField]
        private LinearMap<EntityFamily, ImpactEffectData> _familyToData = null!;

        public void PlayEffects(in ImpactContext context)
        {
            if (context.Victim != null && _familyToData.TryGetValue(context.Victim.Family, out ImpactEffectData data))
            {
                data.PlayEffects(context);
            }
        }
    }
}