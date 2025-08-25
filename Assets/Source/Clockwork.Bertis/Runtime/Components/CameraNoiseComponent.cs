using UnityEngine;

#nullable enable

namespace Clockwork.Bertis.Components
{
    public enum CameraNoiseStopBehavior
    {
        StopInstantly,
        StopGradually
    }

    public sealed class CameraNoiseComponent : MonoBehaviour
    {
        [SerializeField]
        private Transform? _targetTransform;

        [SerializeField]
        private float _blendDuration;
        private float _blendProgress;

        private float _simulationTime;
        private Vector3 _positionSeed;
        private Vector3 _rotationSeed;

        private Vector3 _blendPosition;
        private Quaternion _blendRotation;

        private CameraNoiseDescriptor? _targetDescriptor;
        private CameraNoiseDescriptor? _stableDescriptor;

        private CameraNoiseDescriptor StableDescriptor
        {
            get
            {
                if (_stableDescriptor == null)
                {
                    _stableDescriptor = CameraNoiseDescriptor.CreateStable();
                    _stableDescriptor.BlendDuration = _blendDuration;
                }
                return _stableDescriptor;
            }
        }

        public void OverwriteNoise(CameraNoiseDescriptor? descriptor)
        {
            if (descriptor != null && _targetTransform != null && descriptor != _targetDescriptor)
            {
                _blendProgress = 0f;
                _targetDescriptor = descriptor;
                _targetTransform.GetLocalPositionAndRotation(out _blendPosition, out _blendRotation);
            }
        }

        public void StopNoise(CameraNoiseStopBehavior behavior)
        {
            switch (behavior)
            {
                case CameraNoiseStopBehavior.StopInstantly:
                    if (_targetTransform != null)
                    {
                        _blendProgress = 1f;
                        _targetDescriptor = null;
                        _targetTransform.ResetLocalPositionAndRotation();
                    }
                    break;
                case CameraNoiseStopBehavior.StopGradually:
                    OverwriteNoise(StableDescriptor);
                    break;
                default:
                    ThrowHelpers.ThrowUndefinedEnumIndex(behavior);
                    break;
            }
        }

        private void Awake()
        {
            _positionSeed = Random.Shared.NextVector3();
            _rotationSeed = Random.Shared.NextVector3();
        }

        private void OnDestroy()
        {
            if (_stableDescriptor != null)
            {
                Destroy(_stableDescriptor);
            }
        }

        private void Update()
        {
            if (_targetTransform != null && _targetDescriptor != null)
            {
                float deltaTime = Time.deltaTime;
                _simulationTime += deltaTime;

                (Vector3 position, Quaternion rotation) = _targetDescriptor.Evaluate(
                    _simulationTime, _positionSeed, _rotationSeed);

                if (_blendProgress < 1f)
                {
                    _blendProgress = Math.Min(1f, _blendProgress + deltaTime / _targetDescriptor.BlendDuration);

                    position = Math.Lerp(_blendPosition, position, _blendProgress);
                    rotation = Math.Nlerp(_blendRotation, rotation, _blendProgress);
                }

                _targetTransform.SetLocalPositionAndRotation(position, rotation);
            }
        }
    }
}