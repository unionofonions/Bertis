using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting;

namespace Clockwork.Bertis.Gameplay
{
    public enum PlayerLocomotionState
    {
        Idle,
        Walk
    }

    public enum PlayerUpperBodyState
    {
        Default,
        Reload
    }

    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Animator))]
    public sealed class PlayerPuppet : Entity, IFirearmOperator
    {
        private static readonly int AnimatorLocoHorId = Animator.StringToHash("LocoHor");
        private static readonly int AnimatorLocoVerId = Animator.StringToHash("LocoVer");
        private static readonly int AnimatorReloadId = Animator.StringToHash("Reload");

        private const float MovementDeadzone = 0.01f;

        [SerializeField]
        private float _walkMovementSpeed;
        [SerializeField]
        private float _strafeMobility;
        [SerializeField]
        private float _backstepMobility;

        [SerializeField]
        private float _walkAccelerationSmoothness;
        [SerializeField]
        private float _walkDecelerationSmoothness;

        private Vector2 _walkDampState;
        private Vector2 _walkDampVelocity;

        [SerializeField]
        private float _blendAccSmoothness;
        [SerializeField]
        private float _blendDecSmoothness;

        private Vector2 _blendDampState;
        private Vector2 _blendDampVelocity;

        private bool _isAccelerating;
        private Vector2 _movingDirection;

        [SerializeField]
        private float _walkRotationSpeed;

        private PlayerLocomotionState _locomotionState;
        private PlayerUpperBodyState _upperBodyState;

        [SerializeField]
        private PlayerCamera _playerCamera;
        [SerializeField]
        private Crosshair _crosshair;
        [SerializeField]
        private Transform _aimTarget;

        private float _verticalAimOffset;

        private Rigidbody _rigidbody;
        private Animator _animator;

        [SerializeField]
        private float _baseFirearmSpread;
        [SerializeField]
        private float _walkFirearmSpreadGain;
        [SerializeField]
        private float _firearmSpreadRecovery;

        private float _currentFirearmSpread;
        private float _locomotionFirearmSpread;
        private float _recoilFirearmSpread;

        [SerializeField]
        private float _firearmSpreadToDeviation;

        [SerializeField]
        private Firearm _firearm;

        [SerializeField]
        private InputActionReference _moveActionReference;
        [SerializeField]
        private InputActionReference _shootActionReference;
        [SerializeField]
        private InputActionReference _reloadActionReference;
        [SerializeField]
        private InputActionReference _cycleFireModeActionReference;

        [SerializeField]
        private bool _isIsoPerspective;

        public event Action<PlayerLocomotionState> LocomotionStateChanged;

        public Firearm Firearm => _firearm;

        float IFirearmOperator.SpreadFactor
        {
            get => _currentFirearmSpread * _firearmSpreadToDeviation;
        }

        private void OnEnable()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();

            _verticalAimOffset = _aimTarget != null ? _aimTarget.localPosition.y : 0f;

            if (_firearm != null)
            {
                _firearm.Owner = this;
                _firearm.FiredRound += OnFirearmFiredRound;
            }
            if (_moveActionReference != null)
            {
                _moveActionReference.action.performed += OnMoveActionStateChanged;
                _moveActionReference.action.canceled += OnMoveActionStateChanged;
            }
            if (_shootActionReference != null)
            {
                _shootActionReference.action.performed += OnShootActionStateChanged;
                _shootActionReference.action.canceled += OnShootActionStateChanged;
            }
            if (_reloadActionReference != null)
            {
                _reloadActionReference.action.performed += OnReloadActionPerformed;
            }
            if (_cycleFireModeActionReference != null)
            {
                _cycleFireModeActionReference.action.performed += OnCycleFireModeActionPerformed;
            }
        }

        private void OnDisable()
        {
            _rigidbody.linearVelocity = Vector3.zero;

            if (_firearm != null)
            {
                _firearm.FiredRound -= OnFirearmFiredRound;
                _firearm.Owner = null;
            }
            if (_moveActionReference != null)
            {
                _moveActionReference.action.performed -= OnMoveActionStateChanged;
                _moveActionReference.action.canceled -= OnMoveActionStateChanged;
            }
            if (_shootActionReference != null)
            {
                _shootActionReference.action.performed -= OnShootActionStateChanged;
                _shootActionReference.action.canceled -= OnShootActionStateChanged;
            }
            if (_reloadActionReference != null)
            {
                _reloadActionReference.action.performed -= OnReloadActionPerformed;
            }
            if (_cycleFireModeActionReference != null)
            {
                _cycleFireModeActionReference.action.performed -= OnCycleFireModeActionPerformed;
            }
        }

        private void FixedUpdate()
        {
            float deltaTime = Time.fixedDeltaTime;
            UpdateLocomotionState(deltaTime);
            UpdateFirearmSpread(deltaTime);
        }

        private void UpdateLocomotionState(float deltaTime)
        {
            transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);

            if (_crosshair != null)
            {
                (Quaternion targetRotation, float aimOffsetZ) = _crosshair.GetActorLookRotation(position, rotation, _verticalAimOffset);
                transform.rotation = rotation = Math.Nlerp(rotation, targetRotation, _walkRotationSpeed * deltaTime);

                if (_aimTarget != null)
                {
                    Vector3 aimOffset = _aimTarget.localPosition;
                    aimOffset.z = aimOffsetZ;
                    _aimTarget.localPosition = aimOffset;
                }
            }

            _blendDampState = Math.Deadzone(Math.SmoothDamp(
                _blendDampState,
                _movingDirection,
                ref _blendDampVelocity,
                _isAccelerating ? _blendAccSmoothness : _blendDecSmoothness,
                deltaTime), MovementDeadzone);

            Vector3 facingDirection = Math.Conjugate(rotation) * GetCameraRelativeMovingDirection(_blendDampState);

            _animator.SetFloat(AnimatorLocoHorId, facingDirection.x);
            _animator.SetFloat(AnimatorLocoVerId, facingDirection.z);

            _walkDampState = Math.Deadzone(Math.SmoothDamp(
                _walkDampState,
                _movingDirection,
                ref _walkDampVelocity,
                _isAccelerating ? _walkAccelerationSmoothness : _walkDecelerationSmoothness,
                deltaTime), MovementDeadzone);

            _rigidbody.linearVelocity = _walkMovementSpeed
                * GetDirectionalSpeedMultiplier(new Vector2(facingDirection.x, facingDirection.z))
                * GetCameraRelativeMovingDirection(_walkDampState);

            var locomotionState = _isAccelerating ? PlayerLocomotionState.Walk : PlayerLocomotionState.Idle;
            if (locomotionState != _locomotionState)
            {
                _locomotionState = locomotionState;
                LocomotionStateChanged?.Invoke(locomotionState);
            }
        }

        private Vector3 GetCameraRelativeMovingDirection(Vector2 direction)
        {
            if (!_isIsoPerspective)
            {
                return new Vector3(direction.x, 0f, direction.y);
            }
            else
            {
                Quaternion cameraRotation = _playerCamera.transform.rotation;

                Vector3 cameraForward = cameraRotation * Vector3.forward;
                Vector3 cameraRight = cameraRotation * Vector3.right;

                cameraForward.y = 0f;
                cameraRight.y = 0f;

                cameraForward.Normalize();
                cameraRight.Normalize();

                return cameraForward * direction.y + cameraRight * direction.x;
            }
        }

        private float GetDirectionalSpeedMultiplier(Vector2 direction)
        {
            if (Math.LengthSq(direction) >= MovementDeadzone * MovementDeadzone)
            {
                var weight = new Vector3(
                    Math.Max(0f, direction.y),
                    Math.Max(0f, -direction.y),
                    Math.Abs(direction.x));

                var multiplier = new Vector3(1f, _backstepMobility, _strafeMobility);
                return Math.Dot(multiplier, weight) / Math.Csum(weight);
            }
            return 0f;
        }

        private void UpdateFirearmSpread(float deltaTime)
        {
            float prevSpread = _currentFirearmSpread;
            float nextSpread = _baseFirearmSpread;

            _locomotionFirearmSpread = Math.LerpSnap(
                _locomotionFirearmSpread,
                _locomotionState is PlayerLocomotionState.Idle ? 0f : _walkFirearmSpreadGain,
                _firearmSpreadRecovery * deltaTime,
                0.05f);

            nextSpread += _locomotionFirearmSpread;

            if (_firearm != null)
            {
                _recoilFirearmSpread = Math.LerpSnap(
                    _recoilFirearmSpread,
                    0f,
                    _firearm.RecoilRecovery * deltaTime,
                    0.05f);

                nextSpread += _firearm.BaseRecoil + _recoilFirearmSpread;
            }

            _currentFirearmSpread = nextSpread;

            if (nextSpread != prevSpread && _crosshair != null)
            {
                _crosshair.SpreadFactor = nextSpread;
            }
        }

        [Preserve]
        private void OnReloadAnimationEnded()
        {
            _upperBodyState = PlayerUpperBodyState.Default;
            _animator.SetBool(AnimatorReloadId, false);

            if (_firearm != null)
            {
                _firearm.Reload();

                if (_shootActionReference != null && _shootActionReference.action.IsPressed())
                {
                    _firearm.PullTrigger();
                }
            }
        }

        [Preserve]
        private void PlayFirearmFoleySound(int sampleIndex)
        {
            if (_firearm != null)
            {
                _firearm.PlayFoleySound(sampleIndex);
            }
        }

        private void OnFirearmFiredRound(Firearm firearm)
        {
            _recoilFirearmSpread += firearm.FireRecoilGain;
        }

        private void OnMoveActionStateChanged(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Performed:
                    _isAccelerating = true;
                    _movingDirection = context.ReadValue<Vector2>();
                    break;
                case InputActionPhase.Canceled:
                    _isAccelerating = false;
                    _movingDirection = Vector2.zero;
                    break;
            }
        }

        private void OnShootActionStateChanged(InputAction.CallbackContext context)
        {
            if (_firearm == null || _upperBodyState is PlayerUpperBodyState.Reload)
            {
                return;
            }
            switch (context.phase)
            {
                case InputActionPhase.Performed:
                    _firearm.PullTrigger();
                    break;
                case InputActionPhase.Canceled:
                    _firearm.ReleaseTrigger();
                    break;
            }
        }

        private void OnReloadActionPerformed(InputAction.CallbackContext context)
        {
            if (_firearm != null && !_firearm.IsMagazineFull && _upperBodyState is PlayerUpperBodyState.Default)
            {
                _upperBodyState = PlayerUpperBodyState.Reload;
                _animator.SetBool(AnimatorReloadId, true);
                _firearm.StartReload();
            }
        }

        private void OnCycleFireModeActionPerformed(InputAction.CallbackContext context)
        {
            if (_firearm != null)
            {
                _firearm.CycleFireMode();
            }
        }
    }
}