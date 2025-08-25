using UnityEngine;

namespace Clockwork.Bertis.Components
{
    [CreateAssetMenu(menuName = "Clockwork/Bertis/Components/Hitmarker Descriptor")]
    public sealed class HitmarkerDescriptor : ScriptableObject
    {
        [field: SerializeField]
        public Gradient HealthToColor { get; private set; }

        [field: SerializeField]
        public AnimationCurve SizeInterpolation { get; private set; }

        [field: SerializeField]
        public float SizeScale { get; private set; }

        [field: SerializeField]
        public float StartRotation { get; private set; }

        [field: SerializeField]
        public bool RandomizeRotation { get; private set; }

        [field: SerializeField]
        public bool RotateToIdentity { get; private set; }

        [field: SerializeField]
        public float Duration { get; private set; }
    }
}