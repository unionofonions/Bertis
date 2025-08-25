using UnityEngine;

namespace Clockwork.Simulation
{
    [CreateAssetMenu(menuName = "Clockwork/Simulation/Time Dilation Descriptor")]
    public sealed class TimeDilationDescriptor : ScriptableObject
    {
        [SerializeField]
        private AnimationCurve _interpolation;

        [SerializeField]
        private float _scale;

        [SerializeField]
        private float _duration;

        public bool IsValid => _scale != 0f && _duration > 0f;

        public float Duration => _duration;

        public float Evaluate(float time) => _scale * _interpolation.Evaluate(time);
    }
}