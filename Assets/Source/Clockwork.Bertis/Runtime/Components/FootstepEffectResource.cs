using UnityEngine;
using Clockwork.Bertis.Gameplay;
using Clockwork.Simulation;

namespace Clockwork.Bertis.Components
{
    [CreateAssetMenu(menuName = "Clockwork/Bertis/Components/Footstep Effect Resource")]
    public class FootstepEffectResource : ImpactEffectResource
    {
        [field: SerializeField]
        [field: PrefabReference]
        public DecalDescriptor Footprint { get; private set; }
    }
}