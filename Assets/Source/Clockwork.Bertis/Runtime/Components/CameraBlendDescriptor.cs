using UnityEngine;

namespace Clockwork.Bertis.Components
{
    [CreateAssetMenu(menuName = "Clockwork/Bertis/Components/Camera Blend Descriptor")]
    public sealed class CameraBlendDescriptor : ScriptableObject
    {
        [field: SerializeField]
        public int Priority { get; internal set; }

        [field: SerializeField]
        public AnimationCurve BlendInterpolation { get; internal set; }

        [field: SerializeField]
        public float BlendDuration { get; internal set; }

        [field: SerializeField]
        public float CameraFieldOfView { get; internal set; }

        [field: SerializeField]
        public Vector3 FollowOffset { get; internal set; }

        [field: SerializeField]
        public float FollowSmoothness { get; internal set; }
    }
}