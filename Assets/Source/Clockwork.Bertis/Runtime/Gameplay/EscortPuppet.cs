using System;
using UnityEngine;
using UnityEngine.AI;

namespace Clockwork.Bertis.Gameplay
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class EscortPuppet : Entity
    {
        private static readonly int AnimatorLocoFactorId = Animator.StringToHash("LocoFactor");

        private const float RepositionDeltaThreshold = 0.5f;

        [SerializeField]
        private float _repathInterval;
        private float _repathTimestamp;

        [SerializeField]
        private float _maxMovementSpeed;
        [SerializeField]
        private AnimationCurve _locoFactorToSpeed;

        [SerializeField]
        private float _walkDistance;
        [SerializeField]
        private float _runDistance;

        [SerializeField]
        private float _panicFollowDistance;
        [SerializeField]
        private float _panicRepositionBlendSpeed;

        [SerializeField]
        private float _enemyDetectInterval;
        private float _enemyDetectTimestamp;

        [SerializeField]
        private float _enemyDetectRadius;
        private bool _isEnemyNearby;

        private Transform _followTransform;

        private float _locoFactor;

        private Animator _animator;
        private NavMeshAgent _navAgent;

        public event Action<bool> CombatStatusChanged;

        public void BeginFollow(Transform target)
        {
            if (target == null || IsDead)
            {
                return;
            }
            _followTransform = target;
        }

        public void StopFollow()
        {
            _followTransform = null;
            _navAgent.isStopped = true;
            _navAgent.ResetPath();
            _locoFactor = 0f;
            _animator.SetFloat(AnimatorLocoFactorId, 0f);
        }

        protected override void OnTookFatalDamage(in ImpactContext context)
        {
            base.OnTookFatalDamage(context);
            _navAgent.isStopped = true;
            _navAgent.ResetPath();
        }

        protected void Awake()
        {
            _animator = GetComponent<Animator>();
            _navAgent = GetComponent<NavMeshAgent>();
        }

        protected void Update()
        {
            if (_followTransform == null || IsDead)
            {
                return;
            }

            Vector3 followPosition = _followTransform.position;
            Vector3 selfPosition = transform.position;
            Vector3 deltaPosition = followPosition - selfPosition;

            if ((Time.time - _enemyDetectTimestamp) >= _enemyDetectInterval)
            {
                _enemyDetectTimestamp = Time.time;

                bool isEnemyNearby = Physics.CheckSphere(
                    selfPosition,
                    _enemyDetectRadius,
                    ActorLayers.EnemyMask,
                    QueryTriggerInteraction.Ignore);

                if (isEnemyNearby != _isEnemyNearby)
                {
                    _isEnemyNearby = isEnemyNearby;
                    CombatStatusChanged?.Invoke(isEnemyNearby);
                }
            }

            float distanceSq = Math.LengthSq(deltaPosition);
            float locoFactor;

            if (_isEnemyNearby)
            {
                Quaternion followRotation = _followTransform.rotation;

                Vector3 backLeftPosition = followPosition + followRotation * new Vector3(-0.707f, 0f, -0.707f) * _panicFollowDistance;
                Vector3 backRightPosition = followPosition + followRotation * new Vector3(0.707f, 0f, -0.707f) * _panicFollowDistance;

                Vector3 targetPosition = Math.LengthSq(selfPosition - backLeftPosition) < Math.LengthSq(selfPosition - backRightPosition)
                    ? backLeftPosition : backRightPosition;

                if (Math.Nearby(selfPosition, targetPosition, RepositionDeltaThreshold))
                {
                    locoFactor = Math.LerpSnap(_locoFactor, 0f, Time.deltaTime * _panicRepositionBlendSpeed, 0.05f);
                }
                else
                {
                    locoFactor = Math.LerpSnap(_locoFactor, 1f, Time.deltaTime * _panicRepositionBlendSpeed, 0.05f);
                    UpdateDestination(targetPosition);
                }
            }
            else if (distanceSq <= _walkDistance * _walkDistance + RepositionDeltaThreshold * RepositionDeltaThreshold)
            {
                locoFactor = 0f;
            }
            else
            {
                float distance = Math.Sqrt(distanceSq);
                locoFactor = Math.Unlerp(_walkDistance, _runDistance, distance);
                UpdateDestination(followPosition - deltaPosition.normalized * Math.Min(distance, _walkDistance));
            }

            if (_locoFactor != locoFactor)
            {
                _locoFactor = locoFactor;
                _animator.SetFloat(AnimatorLocoFactorId, locoFactor);
                _navAgent.speed = _locoFactorToSpeed.Evaluate(locoFactor) * _maxMovementSpeed;
                _navAgent.isStopped = locoFactor == 0f;
            }
        }

        private void UpdateDestination(Vector3 value)
        {
            if ((Time.time - _repathTimestamp) >= _repathInterval)
            {
                _repathTimestamp = Time.time;
                _navAgent.destination = value;
            }
        }
    }
}