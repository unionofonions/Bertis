using System;
using System.Buffers;
using UnityEngine;
using UnityEngine.Events;
using Clockwork.Bertis.Components;
using Clockwork.Pooling;
using Clockwork.Simulation;

#nullable enable

namespace Clockwork.Bertis.Gameplay;

[Serializable]
public class ExplosionDefinition
{
    private const int MaxAffectedEntities = 16;

    [SerializeField]
    private float _impactRadius;

    [SerializeField]
    private ImpactEffectResource? _impactEffectResource;

    [SerializeField]
    [PrefabReference]
    private ParticleDescriptor? _explosionParticle;

    [SerializeField]
    private SoundDescriptor? _explosionSound;

    [SerializeField]
    [PrefabReference]
    private PostProcessingDescriptor? _explosionPostProcessing;

    [SerializeField]
    private CameraShakeDescriptor? _explosionShake;

    [SerializeField]
    private PlayerCamera? _playerCamera;

    [SerializeField]
    private UnityEvent _exploded = null!;

    [SerializeField]
    private UnityEvent _rebuilt = null!;

    public float ImpactRadius => _impactRadius;

    public void Explode(Entity instigator, Vector3 position)
    {
        if (instigator == null || _impactRadius <= 0f)
        {
            return;
        }

        Collider[] hitResults = ArrayPool<Collider>.Shared.Rent(MaxAffectedEntities);
        try
        {
            int length = Physics.OverlapSphereNonAlloc(
                position,
                _impactRadius,
                hitResults,
                ActorLayers.PlayerMask | ActorLayers.EnemyMask | ActorLayers.FriendMask,
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < length; i++)
            {
                if (hitResults[i].TryGetComponent(out Entity target) && instigator != target)
                {
                    Vector3 targetPosition = target.transform.position;

                    var context = new ImpactContext(
                        instigator,
                        target,
                        targetPosition,
                        Vector3.up,
                        (targetPosition - position).normalized);

                    target.TakeDamage(context);

                    if (_impactEffectResource != null)
                    {
                        _impactEffectResource.PlayEffects(context);
                    }
                }
            }
        }
        finally
        {
            ArrayPool<Collider>.Shared.Return(hitResults);
        }

        if (_playerCamera != null)
        {
            _playerCamera.StartShake(_explosionShake, position);
        }
        if (PrefabRegistry.Rent(_explosionParticle, out ParticleDescriptor? explosionParticle))
        {
            explosionParticle.Play(position);
        }

        SoundOutputModel.Play(_explosionSound);
        PostProcessingSystem.StartAnimation(_explosionPostProcessing);

        _exploded?.Invoke();
    }

    public void Rebuild() => _rebuilt?.Invoke();
}