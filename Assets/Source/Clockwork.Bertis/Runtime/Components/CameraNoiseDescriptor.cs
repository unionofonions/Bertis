using System.Runtime.CompilerServices;
using UnityEngine;

namespace Clockwork.Bertis.Components
{
    [CreateAssetMenu(menuName = "Clockwork/Bertis/Components/Camera Noise Descriptor")]
    public sealed class CameraNoiseDescriptor : ScriptableObject
    {
        [SerializeField]
        private Vector3 _positionAmplitude;
        [SerializeField]
        private Vector3 _rotationAmplitude;

        [SerializeField]
        private Vector3 _positionFrequency;
        [SerializeField]
        private Vector3 _rotationFrequency;

        [field: SerializeField]
        public float BlendDuration { get; internal set; }

        internal static CameraNoiseDescriptor CreateStable()
            => CreateInstance<CameraNoiseDescriptor>();

        public (Vector3 Position, Quaternion Rotation) Evaluate(float time, Vector3 positionSeed, Vector3 rotationSeed)
        {
            return new(
                Evaluate(time, _positionAmplitude, _positionFrequency, positionSeed),
                Math.FromEuler(Evaluate(time, _rotationAmplitude, _rotationFrequency, rotationSeed)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector3 Evaluate(float time, Vector3 amplitude, Vector3 frequency, Vector3 seed)
        {
            return new(
                amplitude.x == 0f || frequency.x == 0f ? 0f : Math.Perlin(time * frequency.x, seed.x) * amplitude.x,
                amplitude.y == 0f || frequency.y == 0f ? 0f : Math.Perlin(time * frequency.y, seed.y) * amplitude.y,
                amplitude.z == 0f || frequency.z == 0f ? 0f : Math.Perlin(time * frequency.z, seed.z) * amplitude.z);
        }
    }
}