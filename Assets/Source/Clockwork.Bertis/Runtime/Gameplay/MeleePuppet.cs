using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Scripting;
using Clockwork.Collections;
using Clockwork.Simulation;

namespace Clockwork.Bertis.Gameplay
{
    public enum MeleeLocomotionState
    {
        Idle,
        Run,
        Stumble
    }

    public enum MeleeUpperBodyState
    {
        Default,
        Attack,
        Stagger
    }

    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class MeleePuppet : Entity
    {
        private static readonly int AnimatorRunId = Animator.StringToHash("Run");
        private static readonly int AnimatorRunSpeedId = Animator.StringToHash("RunSpeed");
        private static readonly int AnimatorAttackIndexId = Animator.StringToHash("AttackIndex");
        private static readonly int AnimatorStaggerId = Animator.StringToHash("Stagger");
        private static readonly int AnimatorStumbleId = Animator.StringToHash("Stumble");

        private const int AttackAnimationCount = 3;

        [SerializeField]
        private float _repathInterval;
        private float _repathTimestamp;

        [SerializeField]
        private float _attackRange;
        [SerializeField]
        private float _standAttackRange;

        [SerializeField]
        [Range(0f, 1f)]
        private float _attackMoveMobility;
        [SerializeField]
        [Range(0f, 1f)]
        private float _attackAnimMobility;

        [SerializeField]
        private float _attackRotationSped;

        [SerializeField]
        private float _attackCooldown;
        private float _attackTimestamp;

        [SerializeField]
        private Transform _attackHand;
        [SerializeField]
        private float _attackRadius;

        [SerializeField]
        private bool _staggerInterruptsAttack;

        [SerializeField]
        [Range(0f, 1f)]
        private float _staggerProbability;

        [SerializeField]
        private float _staggerCooldown;
        private float _staggerTimestamp;

        [SerializeField]
        [Range(0f, 1f)]
        private float _stumbleProbability;
        [SerializeField]
        private float _stumbleHitArc;

        [SerializeField]
        private float _stumbleCooldown;
        private float _stumbleTimestamp;

        private Transform _preyTransform;

        private float _defaultMoveSpeed;

        private MeleeLocomotionState _locomotionState;
        private MeleeUpperBodyState _upperBodyState;

        private Animator _animator;
        private NavMeshAgent _navAgent;

        [SerializeField]
        private ImpactEffectResource _effectResource;

        [SerializeField]
        private SoundDescriptor _stumbleSound;
        [SerializeField]
        private SoundDescriptor _attackSound;

        [SerializeField]
        private float _soundInterval;
        private float _soundTimestamp;

        private static readonly Collider[] s_overlapSphereResultBuffer = new Collider[8];
        private static readonly UniqueSampler<int> s_attackIndexSampler = CreateAttackIndexSampler();

        public void BeginChase(Transform target)
        {
            if (target == null || IsDead)
            {
                return;
            }

            _preyTransform = target;
            UpdateDestination(force: true);
        }

        public void StopChase()
        {
            _preyTransform = null;
            _navAgent.isStopped = true;
            _navAgent.ResetPath();
            SetLocomotionState(MeleeLocomotionState.Idle);
            SetUpperBodyState(MeleeUpperBodyState.Default);
        }

        protected override void OnTookNonFatalDamage(in ImpactContext context)
        {
            base.OnTookNonFatalDamage(context);

            if (ShouldStumble(context.ContactDirection))
            {
                SetLocomotionState(MeleeLocomotionState.Stumble);
            }
            else if (ShouldStagger())
            {
                SetUpperBodyState(MeleeUpperBodyState.Stagger);
            }
        }

        protected override void OnTookFatalDamage(in ImpactContext context)
        {
            base.OnTookFatalDamage(context);
            StopChase();
        }

        protected void Awake()
        {
            _animator = GetComponent<Animator>();
            _navAgent = GetComponent<NavMeshAgent>();
            _defaultMoveSpeed = _navAgent.speed;
        }

        protected void Update()
        {
            if (_preyTransform == null || _locomotionState == MeleeLocomotionState.Stumble)
            {
                return;
            }

            Vector3 preyPosition = _preyTransform.position;
            Vector3 selfPosition = transform.position;
            UpdateDestination(force: false);

            if (Math.Nearby(selfPosition, preyPosition, _attackRadius) && CanAttack())
            {
                SetUpperBodyState(MeleeUpperBodyState.Attack);
            }

            var locomotionState = Math.Nearby(selfPosition, preyPosition, _standAttackRange)
                ? MeleeLocomotionState.Idle : MeleeLocomotionState.Run;

            if (locomotionState == MeleeLocomotionState.Idle)
            {
                transform.rotation = Math.Nlerp(
                    transform.rotation,
                    Quaternion.LookRotation(preyPosition - selfPosition),
                    _attackRotationSped * Time.deltaTime);
            }

            SetLocomotionState(locomotionState);
        }

        protected void OnValidate()
        {
            _repathInterval = Math.Max(0f, _repathInterval);
            _attackRange = Math.Max(0f, _attackRange);
            _standAttackRange = Math.Clamp(_standAttackRange, 0f, _attackRange);
        }

        protected void OnDrawGizmos()
        {
            if (_attackHand != null)
            {
                var temp = Gizmos.color;
                Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
                Gizmos.DrawSphere(_attackHand.position, _attackRadius);
                Gizmos.color = temp;
            }
        }

        private void UpdateDestination(bool force)
        {
            Debug.Assert(_preyTransform != null);
            if (force || (Time.time - _repathTimestamp) >= _repathInterval)
            {
                _repathTimestamp = Time.time;
                _ = _navAgent.SetDestination(_preyTransform.position);
            }
        }

        private void SetLocomotionState(MeleeLocomotionState state)
        {
            if (state == _locomotionState)
            {
                return;
            }

            switch (_locomotionState)
            {
                case MeleeLocomotionState.Run:
                    _animator.SetBool(AnimatorRunId, false);
                    break;
                case MeleeLocomotionState.Stumble:
                    _stumbleTimestamp = Time.time;
                    _navAgent.isStopped = false;
                    _animator.applyRootMotion = false;
                    _animator.SetBool(AnimatorStumbleId, false);
                    break;
            }

            switch (state)
            {
                case MeleeLocomotionState.Run:
                    _animator.SetBool(AnimatorRunId, true);
                    break;
                case MeleeLocomotionState.Stumble:
                    _navAgent.isStopped = true;
                    _animator.applyRootMotion = true;
                    _animator.SetBool(AnimatorStumbleId, true);
                    PlaySound(_stumbleSound);
                    break;
            }

            _locomotionState = state;
        }

        private void SetUpperBodyState(MeleeUpperBodyState state)
        {
            if (state == _upperBodyState)
            {
                return;
            }

            switch (_upperBodyState)
            {
                case MeleeUpperBodyState.Attack:
                    _attackTimestamp = Time.time;
                    _navAgent.speed = _defaultMoveSpeed;
                    _animator.SetFloat(AnimatorRunSpeedId, 1f);
                    _animator.SetInteger(AnimatorAttackIndexId, 0);
                    break;
                case MeleeUpperBodyState.Stagger:
                    _staggerTimestamp = Time.time;
                    _animator.SetBool(AnimatorStaggerId, false);
                    break;
            }

            switch (state)
            {
                case MeleeUpperBodyState.Attack:
                    _navAgent.speed = _defaultMoveSpeed * _attackMoveMobility;
                    _animator.SetFloat(AnimatorRunSpeedId, _attackAnimMobility);
                    _animator.SetInteger(AnimatorAttackIndexId, s_attackIndexSampler.Sample());
                    PlaySound(_attackSound);
                    break;
                case MeleeUpperBodyState.Stagger:
                    _animator.SetBool(AnimatorStaggerId, true);
                    break;
            }

            _upperBodyState = state;
        }

        private bool CanAttack()
        {
            return _upperBodyState != MeleeUpperBodyState.Attack
                && (Time.time - _attackTimestamp) >= _attackCooldown
                && _locomotionState != MeleeLocomotionState.Stumble;
        }

        private bool ShouldStagger()
        {
            return _upperBodyState != MeleeUpperBodyState.Stagger
                && (Time.time - _staggerTimestamp) >= _staggerCooldown
                && (_staggerInterruptsAttack || _upperBodyState != MeleeUpperBodyState.Attack)
                && Random.Shared.NextBoolean(_staggerProbability);
        }

        private bool ShouldStumble(Vector3 hitDirection)
        {
            return _locomotionState != MeleeLocomotionState.Stumble
                && (Time.time - _stumbleTimestamp) >= _stumbleCooldown
                && Random.Shared.NextBoolean(_stumbleProbability)
                && Math.Dot(transform.forward, -hitDirection) >= Math.Cos(Math.Deg2Rad / 2f * _stumbleHitArc);
        }

        [Preserve]
        private void OnAttackAnimationPerformed()
        {
            if (_attackHand == null)
            {
                return;
            }

            var handPosition = _attackHand.position;
            int bufferLen = Physics.OverlapSphereNonAlloc(
                _attackHand.position,
                _attackRadius,
                s_overlapSphereResultBuffer,
                ActorLayers.PlayerMask | ActorLayers.FriendMask);

            if (bufferLen == 0)
            {
                Debug.LogTrace($"{name} missed attack.");
            }

            for (int i = 0; i < bufferLen; i++)
            {
                if (s_overlapSphereResultBuffer[i].TryGetComponent(out Entity target))
                {
                    var targetPosition = target.transform.position;
                    var context = new ImpactContext(
                        this,
                        target,
                        targetPosition,
                        Vector3.up,
                        (targetPosition - handPosition).normalized);

                    target.TakeDamage(context);
                    if (_effectResource != null)
                    {
                        _effectResource.PlayEffects(context);
                    }

                    Debug.LogTrace($"{name} hit {target.name}");
                }
                else
                {
                    Debug.LogWarning("No entity found.");
                }
            }
        }

        [Preserve]
        private void OnAttackAnimationEnded()
            => SetUpperBodyState(MeleeUpperBodyState.Default);

        [Preserve]
        private void OnStaggerAnimationEnded()
            => SetUpperBodyState(MeleeUpperBodyState.Default);

        [Preserve]
        private void OnStumbleAnimationEnded()
            => SetLocomotionState(MeleeLocomotionState.Idle);

        private void PlaySound(SoundDescriptor descriptor)
        {
            if ((Time.time - _soundTimestamp) >= _soundInterval)
            {
                _soundTimestamp = Time.time;
                SoundOutputModel.Play(descriptor);
            }
        }

        private static UniqueSampler<int> CreateAttackIndexSampler()
        {
            return new UniqueSampler<int>(CreateItems(), copyItems: false)
            {
                KeepOrder = false,
                UniqueSamples = 1
            };

            static int[] CreateItems()
            {
                var result = new int[AttackAnimationCount];
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = i + 1;
                }
                return result;
            }
        }
    }
}