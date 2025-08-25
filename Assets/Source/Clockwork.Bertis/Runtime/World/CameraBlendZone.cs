using UnityEngine;
using Clockwork.Bertis.Components;
using Clockwork.Bertis.Gameplay;

namespace Clockwork.Bertis.World
{
    public class CameraBlendZone : MonoBehaviour
    {
        [SerializeField]
        private CameraBlendDescriptor _blendDescriptor;

        [SerializeField]
        private PlayerCamera _playerCamera;

        private void OnTriggerEnter(Collider other)
        {
            if (_playerCamera != null)
            {
                _playerCamera.QueueBlend(_blendDescriptor);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_playerCamera != null)
            {
                _playerCamera.AbortBlend(_blendDescriptor);
            }
        }
    }
}