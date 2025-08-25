using UnityEngine;
using Clockwork.Collections;
using Clockwork.Simulation;

namespace Clockwork.Bertis.Components
{
    [CreateAssetMenu(menuName = "Clockwork/Bertis/Components/Gore Effect Resource")]
    public sealed class GoreEffectResource : ScriptableObject
    {
        [field: SerializeField]
        public UniqueSampler<DecalDescriptor> HitSplattersWall { get; private set; }

        [field: SerializeField]
        public UniqueSampler<DecalDescriptor> HitSplattersGround { get; private set; }

        [field: SerializeField]
        public UniqueSampler<DecalDescriptor> KillSplattersWall { get; private set; }

        [field: SerializeField]
        public UniqueSampler<DecalDescriptor> KillSplattersGround { get; private set; }
    }
}