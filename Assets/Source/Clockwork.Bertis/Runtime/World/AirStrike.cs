using UnityEngine;
using Clockwork.Bertis.Gameplay;
using Clockwork.Pooling;
using Clockwork.Simulation;

namespace Clockwork.Bertis.World
{
    public class AirStrike : Entity
    {
        [SerializeField]
        private Quaternion _strikeDirection;

        [SerializeField]
        private float _strikeHeight;

        [SerializeField]
        [PrefabReference]
        private Projectile _projectile;

        [SerializeField]
        private int _projectileCount;

        [SerializeField]
        private float _projectileSpeed;

        [SerializeField]
        private float _projectileDeviation;

        [SerializeField]
        private int _fireRate;

        [SerializeField]
        private SoundDescriptor _fireSound;

        [SerializeField]
        [PrefabReference]
        private PostProcessingDescriptor _postProcessing;

        public void BeginStrike(Vector3 pointA, Vector3 pointB)
        {
            Debug.LogInformation($"AirStrike.BeginStrike has started: point a: {pointA}, point b: {pointB}.");
            SoundOutputModel.Play(_fireSound);
            PostProcessingSystem.StartAnimation(_postProcessing);
            _ = StrikeAsync(pointA, pointB);
        }

        private async Awaitable StrikeAsync(Vector3 pointA, Vector3 pointB)
        {
            Vector3 forward = _strikeDirection * Vector3.forward;
            float fireInterval = 60f / _fireRate;

            for (int i = 0; i < _projectileCount; i++)
            {
                Vector3 targetPosition = Math.Lerp(pointA, pointB, (float)i / _projectileCount);
                float backtrack = (_strikeHeight - targetPosition.y) / -forward.y;

                Vector3 launchPosition = targetPosition - forward * backtrack;
                Vector2 deviation = Random.Shared.NextVector2(_projectileDeviation * Vector2.one);
                launchPosition += _strikeDirection * new Vector3(deviation.x, 0f, deviation.y);

                ShootProjectile(launchPosition);
                await Awaitable.WaitForSecondsAsync(fireInterval);
            }
        }

        private void ShootProjectile(Vector3 position)
        {
            if (PrefabRegistry.Rent(_projectile, out Projectile projectile))
            {
                var context = new ProjectileShootContext(
                    this,
                    position,
                    _strikeDirection,
                    _projectileSpeed,
                    _strikeHeight * 2f);

                projectile.Shoot(context);
            }
        }
    }
}