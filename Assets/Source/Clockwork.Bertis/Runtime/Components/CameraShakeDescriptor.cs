using UnityEngine;

namespace Clockwork.Bertis.Components
{
    [CreateAssetMenu(menuName = "Clockwork/Bertis/Components/Camera Shake Descriptor")]
    public sealed class CameraShakeDescriptor : ScriptableObject
    {
        [SerializeField]
        private AnimationCurve _positionCurveX;
        [SerializeField]
        private AnimationCurve _positionCurveY;
        [SerializeField]
        private AnimationCurve _positionCurveZ;

        [SerializeField]
        private AnimationCurve _rotationCurveX;
        [SerializeField]
        private AnimationCurve _rotationCurveY;
        [SerializeField]
        private AnimationCurve _rotationCurveZ;

        [SerializeField]
        private Vector3 _positionScale;
        [SerializeField]
        private Vector3 _rotationScale;

        [SerializeField]
        private AnimationCurve _distanceToScale;
        [SerializeField]
        [Range(0f, 1f)]
        private float _spatialBlend;

        [SerializeField]
        [Range(0f, 1f)]
        private float _timeDilationFactor;

        [SerializeField]
        private float _duration;

        [SerializeField]
        [DesignerHidden]
        private int _validFlags;

        public float Duration => _duration;

        public bool IsSpatial => _spatialBlend > 0f;

        public bool IsValid => _validFlags != 0;

        public (Vector3 Position, Vector3 Rotation) Evaluate(float time)
        {
            (Vector3 position, Vector3 rotation) = (Vector3.zero, Vector3.zero);
            int validFlags = _validFlags;

            if ((validFlags & 7) != 0)
            {
                if ((validFlags & 1) != 0)
                {
                    position.x = _positionCurveX.Evaluate(time);
                }
                if ((validFlags & 2) != 0)
                {
                    position.y = _positionCurveY.Evaluate(time);
                }
                if ((validFlags & 4) != 0)
                {
                    position.z = _positionCurveZ.Evaluate(time);
                }
                position = Math.Mul(position, _positionScale);
            }

            if ((validFlags & 56) != 0)
            {
                if ((validFlags & 8) != 0)
                {
                    rotation.x = _rotationCurveX.Evaluate(time);
                }
                if ((validFlags & 16) != 0)
                {
                    rotation.y = _rotationCurveY.Evaluate(time);
                }
                if ((validFlags & 32) != 0)
                {
                    rotation.z = _rotationCurveZ.Evaluate(time);
                }
                rotation = Math.Mul(rotation, _rotationScale);
            }

            return (position, rotation);
        }

        public float ResolveScale(Vector3 source, Vector3 target)
        {
            float distance = Math.Distance(source, target);
            return _spatialBlend + (1f - _spatialBlend) * _distanceToScale.Evaluate(distance);
        }

        public float ResolveDeltaTime((float Scaled, float Unscaled) deltaTime)
            => (1f - _timeDilationFactor) * deltaTime.Scaled + _timeDilationFactor * deltaTime.Unscaled;

        private void OnValidate()
        {
            _validFlags = 0;

            if (_duration > 0f)
            {
                if (IsAxisValid(_positionCurveX, _positionScale.x))
                {
                    _validFlags = 1;
                }
                if (IsAxisValid(_positionCurveY, _positionScale.y))
                {
                    _validFlags |= 2;
                }
                if (IsAxisValid(_positionCurveZ, _positionScale.z))
                {
                    _validFlags |= 4;
                }

                if (IsAxisValid(_rotationCurveX, _rotationScale.x))
                {
                    _validFlags |= 8;
                }
                if (IsAxisValid(_rotationCurveY, _rotationScale.y))
                {
                    _validFlags |= 16;
                }
                if (IsAxisValid(_rotationCurveZ, _rotationScale.z))
                {
                    _validFlags |= 32;
                }
            }
        }

        private static bool IsAxisValid(AnimationCurve curve, float scale)
            => curve.length > 0 && scale != 0f;
    }
}