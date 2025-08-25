using UnityEngine;
using Clockwork.Bertis.Components;
using Clockwork.Bertis.Gameplay;
using Clockwork.Pooling;
using Clockwork.Simulation;

namespace Clockwork.Bertis
{
    internal class Dummy : Entity
    {
        [SerializeField]
        private Transform _startPoint;
        [SerializeField]
        private Transform _endPoint;
        [SerializeField]
        private Transform _strikeDirection;

        [SerializeField]
        private int _strikeCount;
        [SerializeField]
        private float _strikeInterval;

        [SerializeField]
        [PrefabReference]
        private Projectile _strikeProjectile;
        [SerializeField]
        private float _projectileSpeed;

        [SerializeField]
        private float _strikeHeight;
        [SerializeField]
        private float _strikeNoise;

        [SerializeField]
        private AudioSource _audioSource;

        [SerializeField]
        private SoundDescriptor _debrisSound;
        [SerializeField]
        private float _debrisDelay;

        [SerializeField]
        private CameraShakeDescriptor _strikeShake;
        [SerializeField]
        private float _shakeDelay;

        [SerializeField]
        private CameraBlendDescriptor _startCameraBlend;

        [SerializeField]
        private float _cameraBlendDelay;

        [SerializeField]
        private PlayerCamera _playerCamera;

        [SerializeField]
        [PrefabReference]
        private PostProcessingDescriptor _strikePostProcessing;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                _audioSource.Play();
                PostProcessingSystem.StartAnimation(_strikePostProcessing);
                _playerCamera.QueueBlend(_startCameraBlend);
                StartCoroutine(Strike());
            }
        }

        private System.Collections.IEnumerator Strike()
        {
            Vector3 startPoint = _startPoint.position;
            Vector3 endPoint = _endPoint.position;

            Quaternion strikeRotation = _strikeDirection.localRotation;
            Vector3 strikeForward = strikeRotation * Vector3.forward;

            for (int i = 0; i < _strikeCount; i++)
            {
                Vector3 targetPosition = Math.Lerp(startPoint, endPoint, (float)i / _strikeCount);
                float backtrack = (_strikeHeight - targetPosition.y) / -strikeForward.y;

                Vector3 launchPosition = targetPosition - strikeForward * backtrack;
                Vector2 deviation = Random.Shared.NextVector2(_strikeNoise * Vector2.one);
                launchPosition += strikeRotation * new Vector3(deviation.x, 0f, deviation.y);

                if (PrefabRegistry.Rent(_strikeProjectile, out Projectile strikeProjectile))
                {
                    var context = new ProjectileShootContext(
                        this,
                        launchPosition,
                        strikeRotation,
                        _projectileSpeed,
                        _strikeHeight * 2f);

                    strikeProjectile.Shoot(context);
                }

                yield return new WaitForSeconds(_strikeInterval);
            }

            yield return new WaitForSeconds(_shakeDelay);
            _playerCamera.StartShake(_strikeShake);
            yield return new WaitForSeconds(_debrisDelay);
            SoundOutputModel.Play(_debrisSound);
            yield return new WaitForSeconds(_cameraBlendDelay);
            _playerCamera.AbortBlend(_startCameraBlend);
        }
    }
}