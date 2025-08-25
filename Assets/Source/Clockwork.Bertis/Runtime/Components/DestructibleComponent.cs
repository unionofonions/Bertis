using UnityEngine;
using UnityEngine.Events;
using Clockwork.Bertis.Gameplay;
using Clockwork.Simulation;

namespace Clockwork.Bertis.Components
{
    public class DestructibleComponent : MonoBehaviour
    {
        [SerializeField]
        private GameObject _originalMesh;

        [SerializeField]
        private GameObject _destructedMesh;

        [SerializeField]
        private ParticleSystem _explosionParticle;

        [SerializeField]
        private ParticleSystem _debrisParticle;

        [SerializeField]
        private SoundDescriptor _explosionSound;

        [SerializeField]
        private CameraShakeDescriptor _explosionShake;

        [SerializeField]
        private PlayerCamera _playerCamera;

        [SerializeField]
        [PrefabReference]
        private PostProcessingDescriptor _explodePostProcessing;

        [SerializeField]
        private UnityEvent _exploded;

        public void Explode()
        {
            _originalMesh.SetActive(false);
            _destructedMesh.SetActive(true);

            if (_explosionParticle != null)
            {
                _explosionParticle.Play();
            }
            if (_debrisParticle != null)
            {
                _debrisParticle.Play();
            }
            if (_playerCamera != null)
            {
                _playerCamera.StartShake(_explosionShake);
            }

            PostProcessingSystem.StartAnimation(_explodePostProcessing);
            SoundOutputModel.Play(_explosionSound);
            _exploded.Invoke();
        }

        public void ReverseExplode()
        {
            _originalMesh.SetActive(true);
            _destructedMesh.SetActive(false);
        }
    }
}