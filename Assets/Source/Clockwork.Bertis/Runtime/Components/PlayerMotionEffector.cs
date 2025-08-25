using UnityEngine;
using Clockwork.Bertis.Gameplay;

namespace Clockwork.Bertis.Components
{
    [RequireComponent(typeof(PlayerPuppet))]
    public sealed class PlayerMotionEffector : MonoBehaviour
    {
        [SerializeField]
        private CameraBlendDescriptor _playerWalkCameraBlend;

        [SerializeField]
        private CameraNoiseDescriptor _playerIdleCameraNoise;
        [SerializeField]
        private CameraNoiseDescriptor _playerWalkCameraNoise;

        [SerializeField]
        private PlayerCamera _playerCamera;
        private PlayerPuppet _playerPuppet;

        private void OnEnable()
        {
            _playerPuppet = GetComponent<PlayerPuppet>();
            _playerPuppet.LocomotionStateChanged += OnLocomotionStateChanged;

            if (_playerCamera != null)
            {
                _playerCamera.OverwriteNoise(_playerIdleCameraNoise);
            }
        }

        private void OnDisable()
        {
            _playerPuppet.LocomotionStateChanged -= OnLocomotionStateChanged;

            if (_playerCamera != null)
            {
                _playerCamera.AbortBlend(_playerWalkCameraBlend);
                _playerCamera.StopNoise(CameraNoiseStopBehavior.StopGradually);
            }
        }

        private void OnLocomotionStateChanged(PlayerLocomotionState state)
        {
            if (_playerCamera == null)
            {
                return;
            }
            switch (state)
            {
                case PlayerLocomotionState.Idle:
                    _playerCamera.AbortBlend(_playerWalkCameraBlend);
                    _playerCamera.OverwriteNoise(_playerIdleCameraNoise);
                    break;
                case PlayerLocomotionState.Walk:
                    _playerCamera.QueueBlend(_playerWalkCameraBlend);
                    _playerCamera.OverwriteNoise(_playerWalkCameraNoise);
                    break;
            }
        }
    }
}