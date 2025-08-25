using UnityEngine;
using Clockwork.Bertis.Components;

#nullable enable

namespace Clockwork.Bertis.Gameplay
{
    public sealed class PlayerCamera : MonoBehaviour
    {
        private CameraBlendComponent? _blendComponent;
        private CameraShakeComponent? _shakeComponent;
        private CameraNoiseComponent? _noiseComponent;

        public void QueueBlend(CameraBlendDescriptor? descriptor)
        {
            if (_blendComponent != null)
            {
                _blendComponent.QueueBlend(descriptor);
            }
        }

        public void AbortBlend(CameraBlendDescriptor? descriptor)
        {
            if (_blendComponent != null)
            {
                _blendComponent.AbortBlend(descriptor);
            }
        }

        public void StartShake(CameraShakeDescriptor? descriptor, Vector3 position)
        {
            if (_shakeComponent != null)
            {
                _shakeComponent.StartShake(descriptor, position);
            }
        }

        public void StartShake(CameraShakeDescriptor? descriptor)
        {
            if (_shakeComponent != null)
            {
                _shakeComponent.StartShake(descriptor);
            }
        }

        public void OverwriteNoise(CameraNoiseDescriptor? descriptor)
        {
            if (_noiseComponent != null)
            {
                _noiseComponent.OverwriteNoise(descriptor);
            }
        }

        public void StopNoise(CameraNoiseStopBehavior behavior)
        {
            if (_noiseComponent != null)
            {
                _noiseComponent.StopNoise(behavior);
            }
        }

        private void Awake()
        {
            _blendComponent = GetComponent<CameraBlendComponent>();
            _shakeComponent = GetComponent<CameraShakeComponent>();
            _noiseComponent = GetComponent<CameraNoiseComponent>();
        }
    }
}