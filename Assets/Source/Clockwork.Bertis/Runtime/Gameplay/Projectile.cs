using UnityEngine;
using Clockwork.Pooling;

namespace Clockwork.Bertis.Gameplay
{
    public readonly ref struct ProjectileShootContext
    {
        public readonly Entity Owner;
        public readonly Vector3 LaunchPosition;
        public readonly Quaternion LaunchRotation;
        public readonly float LinearSpeed;
        public readonly float MaxRange;

        public ProjectileShootContext(
            Entity owner,
            Vector3 launchPosition,
            Quaternion launchRotation,
            float linearSpeed,
            float maxRange)
        {
            Owner = owner;
            LaunchPosition = launchPosition;
            LaunchRotation = launchRotation;
            LinearSpeed = linearSpeed;
            MaxRange = maxRange;
        }
    }

    [RequireComponent(typeof(TrailRenderer))]
    public class Projectile : MonoBehaviour
    {
        private float _linearSpeed;
        private Vector3 _movingDirection;
        private float _remainingDistance;

        [SerializeField]
        private ImpactEffectResource _effectResource;

        private bool _isFading;
        private float _fadeDuration;
        private float _fadeTimer;

        private Entity _owner;
        private TrailRenderer _tracer;

        public void Shoot(ProjectileShootContext context)
        {
            transform.SetPositionAndRotation(context.LaunchPosition, context.LaunchRotation);
            gameObject.SetActive(true);
            _linearSpeed = context.LinearSpeed;
            _movingDirection = context.LaunchRotation * Vector3.forward;
            _remainingDistance = context.MaxRange;
            _owner = context.Owner;
            _isFading = false;
        }

        protected void Awake()
        {
            _tracer = GetComponent<TrailRenderer>();
            _fadeDuration = _tracer.time;
        }

        protected void FixedUpdate()
        {
            if (_isFading)
            {
                _fadeTimer -= Time.fixedDeltaTime;
                if (_fadeTimer <= 0f)
                {
                    Deactivate();
                }
                return;
            }

            float step = Math.Min(_remainingDistance, _linearSpeed * Time.fixedDeltaTime);
            Vector3 position = transform.position;
            bool expired;

            if (Physics.Raycast(
                position,
                _movingDirection,
                out RaycastHit hitInfo,
                step,
                ActorLayers.GroundMask | ActorLayers.WallMask | ActorLayers.EnemyMask | ActorLayers.FriendMask | ActorLayers.ExplosiveMask,
                QueryTriggerInteraction.Ignore))
            {
                step = hitInfo.distance;
                expired = true;

                if (hitInfo.collider.TryGetComponent(out Entity target))
                {
                    var context = new ImpactContext(
                        _owner,
                        target,
                        hitInfo.point,
                        hitInfo.normal,
                        _movingDirection);

                    target.TakeDamage(context);

                    if (_effectResource != null)
                    {
                        _effectResource.PlayEffects(context);
                    }
                }
            }
            else
            {
                _remainingDistance -= step;
                expired = _remainingDistance <= 0f;
            }

            transform.position = position + _movingDirection * step;
            if (expired)
            {
                _isFading = true;
                _fadeTimer = _fadeDuration;
            }
        }

        private void Deactivate()
        {
            _tracer.Clear();
            gameObject.SetActive(false);
            PrefabRegistry.Return(this);
        }
    }
}