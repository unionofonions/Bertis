using UnityEngine;

#nullable enable

namespace Clockwork.Bertis.Components
{
    public sealed class CameraFollowComponent : MonoBehaviour
    {
        [SerializeField]
        private Vector3 _followOffset;

        [SerializeField]
        private Vector3 _fixedOffset;

        [SerializeField]
        private float _followSmoothness;

        [SerializeField]
        private Transform? _followTransform;

        private Vector3 _followVelocity;

        [SerializeField]
        private Transform? _targetTransform;

        public Vector3 FollowOffset
        {
            get => _followOffset;
            set => _followOffset = value;
        }

        public float FollowSmoothness
        {
            get => _followSmoothness;
            set => _followSmoothness = value;
        }

        public void SyncWithTarget()
        {
            if (_targetTransform != null && _followTransform != null)
            {
                _followTransform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
                _targetTransform.localPosition = position + _fixedOffset + rotation * _followOffset;
            }
        }

        private void FixedUpdate()
        {
            if (_targetTransform != null && _followTransform != null)
            {
                _followTransform.GetPositionAndRotation(
                    out Vector3 targetPosition,
                    out Quaternion targetRotation);

                _targetTransform.localPosition = Vector3.SmoothDamp(
                    _targetTransform.localPosition,
                    targetPosition + _fixedOffset + targetRotation * _followOffset,
                    ref _followVelocity,
                    _followSmoothness,
                    float.PositiveInfinity,
                    Time.fixedDeltaTime);
            }
        }
    }
}