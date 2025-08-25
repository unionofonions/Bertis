using UnityEngine;
using Clockwork.Bertis.Gameplay;
using Clockwork.Simulation;

#nullable enable

namespace Clockwork.Bertis.World
{
    public class C4 : Entity
    {
        [SerializeField]
        private GameObject? _beacon;

        [SerializeField]
        private float _activationBlinkDuration;
        [SerializeField]
        private float _idleBlinkDuration;
        [SerializeField]
        private float _blinkInterval;

        private bool _isActivated;
        private bool _isDetonated;

        private bool _isBlinking;
        private float _blinkTimer;
        private float _blinkDeltaScale;

        [SerializeField]
        private float _explosionDelay;

        [SerializeField]
        private SoundDescriptor? _activationSound;
        [SerializeField]
        private SoundDescriptor? _tickingSound;

        [SerializeField]
        private ExplosionDefinition _explosionDefinition = null!;

        public void Activate()
        {
            if (_isDetonated)
            {
                Debug.LogError("C4.Activate failed: object already detonated.", context: this);
                return;
            }
            if (_isActivated)
            {
                Debug.LogError("C4.Activate failed: object already activated.", context: this);
                return;
            }

            _isActivated = true;
            _isBlinking = true;
            _blinkTimer = 0f;
            _blinkDeltaScale = 1f / _activationBlinkDuration;
            _beacon?.SetActive(true);
            SoundOutputModel.Play(_activationSound);
        }

        public void Detonate()
        {
            if (_isDetonated)
            {
                Debug.LogError("C4.Detonate failed: object already detonated.", context: this);
                return;
            }
            if (!_isActivated)
            {
                Debug.LogError("C4.Detonate failed: object not activated.", context: this);
                return;
            }

            _isActivated = false;
            _beacon?.SetActive(true);
            SoundOutputModel.Play(_tickingSound);
            _ = DetonateAsync();

        }

        private async Awaitable DetonateAsync()
        {
            await Awaitable.WaitForSecondsAsync(_explosionDelay);
            _explosionDefinition.Explode(this, transform.position);
        }

        internal void Rebuild()
        {
            _isDetonated = false;
            _explosionDefinition.Rebuild();
        }

        protected void Update()
        {
            if (_isActivated)
            {
                _blinkTimer += Time.deltaTime * _blinkDeltaScale;
                if (_blinkTimer >= 1f)
                {
                    bool nextState = !_isBlinking;
                    _blinkTimer = 0f;
                    _blinkDeltaScale = 1f / (nextState ? _idleBlinkDuration : _blinkInterval);
                    _isBlinking = nextState;
                    _beacon?.SetActive(nextState);
                }
            }
        }

        protected void OnDrawGizmos()
        {
            Color temp = Gizmos.color;
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
            Gizmos.DrawSphere(transform.position, _explosionDefinition.ImpactRadius);
            Gizmos.color = temp;
        }
    }
}