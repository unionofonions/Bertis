using UnityEngine;
using Clockwork.Bertis.Gameplay;
using Clockwork.Simulation;

namespace Clockwork.Bertis.Components
{
    public sealed class PlayerStimuliEffector : MonoBehaviour
    {
        private enum EnemyProximity
        {
            None,
            PointBlank,
            Nearby
        }

        [SerializeField]
        private CameraBlendDescriptor _enemyNearbyCameraBlend;

        [SerializeField]
        private CameraBlendDescriptor _enemyPointblankCameraBlend;

        [SerializeField]
        private float _enemyNearbyRange;

        [SerializeField]
        private float _enemyPointblankRange;

        private EnemyProximity _enemyProximity;

        [SerializeField]
        private CameraShakeDescriptor _playerHurtShake;

        [SerializeField]
        private PlayerCamera _playerCamera;

        [SerializeField]
        private TimeDilationDescriptor _playerKillDilation;

        [SerializeField]
        private TimeDilationDescriptor _playerHurtDilation;

        [SerializeField]
        [PrefabReference]
        private PostProcessingDescriptor _playerHurtPostProcessing;

        private void OnEnable()
        {
            Entity.EntityTookDamage += EntityTookDamage;
        }

        private void OnDisable()
        {
            Entity.EntityTookDamage -= EntityTookDamage;
        }

        private void FixedUpdate()
        {
            if (_playerCamera == null)
            {
                return;
            }

            var nextProximity = EnemyProximity.None;

            if (Physics.CheckSphere(
                transform.position,
                _enemyNearbyRange,
                ActorLayers.EnemyMask,
                QueryTriggerInteraction.Ignore))
            {
                nextProximity = EnemyProximity.Nearby;

                if (Physics.CheckSphere(
                transform.position,
                _enemyPointblankRange,
                ActorLayers.EnemyMask,
                QueryTriggerInteraction.Ignore))
                {
                    nextProximity = EnemyProximity.PointBlank;
                }
            }

            if (nextProximity != _enemyProximity)
            {
                switch (_enemyProximity)
                {
                    case EnemyProximity.PointBlank:
                        _playerCamera.AbortBlend(_enemyPointblankCameraBlend);
                        break;
                    case EnemyProximity.Nearby:
                        _playerCamera.AbortBlend(_enemyNearbyCameraBlend);
                        break;
                }

                switch (nextProximity)
                {
                    case EnemyProximity.PointBlank:
                        _playerCamera.QueueBlend(_enemyPointblankCameraBlend);
                        break;
                    case EnemyProximity.Nearby:
                        _playerCamera.QueueBlend(_enemyNearbyCameraBlend);
                        break;
                }

                _enemyProximity = nextProximity;
            }
        }

        private void OnValidate()
        {
            _enemyNearbyRange = Math.Max(0f, _enemyNearbyRange);
            _enemyPointblankRange = Math.Clamp(_enemyPointblankRange, 0f, _enemyNearbyRange);
        }

        private void OnDrawGizmos()
        {
            Color temp = Gizmos.color;

            Gizmos.color = new Color(1f, 0f, 0f, 0.135f);
            Gizmos.DrawSphere(transform.position, _enemyPointblankRange);

            Gizmos.color = new Color(0f, 0f, 1f, 0.115f);
            Gizmos.DrawSphere(transform.position, _enemyNearbyRange);

            Gizmos.color = temp;
        }

        private void EntityTookDamage(in ImpactContext context)
        {
            if (context.IsFatal && context.Instigator is PlayerPuppet)
            {
                switch (context.Victim.Family)
                {
                    case EntityFamily.Zombie:
                    TimeDilationSystem.StartTimeDilation(_playerKillDilation);
                        break;
                    case EntityFamily.Explosive:
                    TimeDilationSystem.StartTimeDilation(_playerKillDilation);
                        break;
                }
            }
            else if (!context.IsFatal && context.Victim is PlayerPuppet)
            {
                TimeDilationSystem.StartTimeDilation(_playerHurtDilation);
                PostProcessingSystem.StartAnimation(_playerHurtPostProcessing);

                if (_playerCamera != null)
                {
                    _playerCamera.StartShake(_playerHurtShake);
                }
            }
        }
    }
}