using UnityEngine;
using UnityEngine.Scripting;
using Clockwork.Bertis.Gameplay;
using Clockwork.Pooling;
using Clockwork.Simulation;

namespace Clockwork.Bertis.Components
{
    public enum FootprintCondition
    {
        Never,
        Always,
        Puddle
    }

    public sealed class FootstepComponent : MonoBehaviour
    {
        [SerializeField]
        private Transform _leftFoot;
        [SerializeField]
        private Transform _rightFoot;

        [SerializeField]
        private FootstepEffectResource _effectResource;

        [SerializeField]
        private FootprintCondition _footprintCondition;

        [SerializeField]
        private int _footprintCount;
        private int _activeFootprints;

        [SerializeField]
        private float _applyInterval;
        private float _applyTimestamp;

        [Preserve]
        private void OnFootstepPerformed(int isLeftFoot)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return;
            }
#endif
            if ((Time.time - _applyTimestamp) < _applyInterval)
            {
                return;
            }
            _applyTimestamp = Time.time;

            var feet = isLeftFoot != 0 ? _leftFoot : _rightFoot;
            var position = feet.position;

            if (Physics.Raycast(
                position,
                Vector3.down,
                out RaycastHit hitInfo,
                1f,
                ActorLayers.GroundMask,
                QueryTriggerInteraction.Ignore) &&
                hitInfo.collider.TryGetComponent(out Entity target))
            {
                var context = new ImpactContext(
                    instigator: null,
                    target,
                    hitInfo.point,
                    hitInfo.normal,
                    Vector3.down);

                _effectResource.PlayEffects(context);

                if ((_footprintCondition == FootprintCondition.Always || _activeFootprints > 0) &&
                    PrefabRegistry.Rent(_effectResource.Footprint, out DecalDescriptor footprint))
                {
                    if (_footprintCondition == FootprintCondition.Puddle)
                    {
                        footprint.AlphaScale = (float)_activeFootprints / _footprintCount;
                        _activeFootprints--;
                    }
                    footprint.FlipHorizontally = isLeftFoot != 0;
                    footprint.PlaceProjected(hitInfo.point, transform.forward, hitInfo.normal);
                }

                if (_footprintCondition == FootprintCondition.Puddle && Physics.CheckSphere(
                    position,
                    0.3f,
                    ActorLayers.PuddleMask,
                    QueryTriggerInteraction.Collide))
                {
                    _activeFootprints = _footprintCount;
                }
            }
        }
    }
}