using UnityEngine;
using Clockwork.Pooling;
using Clockwork.Simulation;

namespace Clockwork.Bertis.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public class Casing : MonoBehaviour, IPoolWorker
    {
        [SerializeField]
        private float _forceMin, _forceMax;

        [SerializeField]
        private Vector3 _torqueMax;

        [SerializeField]
        private Vector3 _rotationMin, _rotationMax;

        private Rigidbody _rigidbody;

        [SerializeField]
        private FadeConfig _fadeConfig;
        private FadeCompositor _fadeCompositor;

        [SerializeField]
        private ImpactEffectResource _effectResource;

        private Entity _ground;
        private float _landTimer;

        private void SetFadeFactor(float value)
            => transform.localScale = value * Vector3.one;

        public void Eject(Vector3 position, Quaternion rotation)
        {
            if (!_fadeCompositor.Start())
            {
                Deactivate();
                return;
            }

            transform.SetPositionAndRotation(position, rotation);

            Vector3 linearVelocity = rotation
                * (Math.FromEuler(Random.Shared.NextVector3(_rotationMin, _rotationMax))
                * new Vector3(0f, 0f, Random.Shared.NextSingle(_forceMin, _forceMax)));

            _rigidbody.linearVelocity = linearVelocity;
            _rigidbody.angularVelocity = Random.Shared.NextVector3(_torqueMax);

            if (Physics.Raycast(
                position,
                Vector3.down,
                out RaycastHit hitInfo,
                float.PositiveInfinity,
                ActorLayers.GroundMask,
                QueryTriggerInteraction.Ignore))
            {
                if (hitInfo.collider.TryGetComponent(out _ground))
                {
                    _landTimer = CalculateLandDelay(linearVelocity.y, hitInfo.distance, 9.81f);
                }
            }

            gameObject.SetActive(true);
        }

        protected void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _fadeCompositor = new FadeCompositor(SetFadeFactor, _fadeConfig);
        }

        protected void Update()
        {
            if (!_fadeCompositor.Update(Time.deltaTime))
            {
                Deactivate();
                return;
            }

            if (_landTimer > 0f)
            {
                _landTimer -= Time.deltaTime;
                if (_landTimer <= 0f && _effectResource != null)
                {
                    var context = new ImpactContext(
                        instigator: null,
                        _ground,
                        transform.position,
                        Vector3.up,
                        Vector3.down);

                    _effectResource.PlayEffects(context);
                }
            }
        }

        private void Deactivate()
        {
            _ground = null;
            gameObject.SetActive(false);
            PrefabRegistry.Return(this);
        }

        void IPoolWorker.ScheduleEarlyReturn()
        {
            if (_fadeCompositor.State != FadeState.FadeOut)
            {
                _fadeCompositor.Start(FadeState.FadeOut);
            }
        }

        private static float CalculateLandDelay(float v, float h, float g)
            => (v + Math.Sqrt(v * v + 2f * g * h)) / g;
    }
}