using System.Threading;
using UnityEngine;
using Clockwork.Pooling;

namespace Clockwork.Simulation
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleDescriptor : MonoBehaviour, IPoolWorker
    {
        [SerializeField]
        [DesignerHidden]
        private float _emitDuration;
        [SerializeField]
        [DesignerHidden]
        private float _playDuration;

        private float _timer;
        private bool _stoppedEmitting;
        private CancellationToken _cancellationToken;

        private ParticleSystem _particleSystem;

        public void Play(Vector3 position, Quaternion rotation, CancellationToken cancellationToken = default)
        {
            transform.SetPositionAndRotation(position, rotation);
            gameObject.SetActive(true);
            _particleSystem.Play();
            _timer = 0f;
            _stoppedEmitting = false;
            _cancellationToken = cancellationToken;
        }

        public void Play(Vector3 position, CancellationToken cancellationToken = default)
        {
            transform.position = position;
            gameObject.SetActive(true);
            _particleSystem.Play();
            _timer = 0f;
            _stoppedEmitting = false;
            _cancellationToken = cancellationToken;
        }

        protected void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        protected void Update()
        {
            if (_cancellationToken.IsCancellationRequested)
            {
                Deactivate();
                gameObject.SetActive(false);
                return;
            }

            _timer += Time.deltaTime;
            if (!_stoppedEmitting && _timer >= _emitDuration)
            {
                Deactivate();
            }
            else if (_timer >= _playDuration)
            {
                _cancellationToken = default;
                gameObject.SetActive(false);
            }
        }

        private void Deactivate()
        {
            _cancellationToken = default;
            _stoppedEmitting = true;
            PrefabRegistry.Return(this);
        }

        void IPoolWorker.ScheduleEarlyReturn()
        {
            Deactivate();
        }
    }
}