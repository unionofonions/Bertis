using UnityEngine;
using Clockwork.Collections;

#nullable enable

namespace Clockwork.Simulation
{
    [CreateAssetMenu(menuName = "Clockwork/Simulation/Sound Descriptor")]
    public class SoundDescriptor : ScriptableObject
    {
        [field: SerializeField]
        public UniqueSampler<AudioClip> Samples { get; private set; } = null!;

        [field: SerializeField]
        [field: Range(0f, 1f)]
        public float VolumeScale { get; private set; } = 1f;

        [field: SerializeField]
        [field: PrefabReference]
        public SoundOutputModel? OutputModel { get; private set; }
    }
}