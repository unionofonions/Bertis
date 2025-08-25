using UnityEngine;
using Clockwork.Bertis.Gameplay;
using Clockwork.Pooling;
using Clockwork.Simulation;

namespace Clockwork.Bertis.Components
{
    [RequireComponent(typeof(Entity))]
    public sealed class GoreComponent : MonoBehaviour
    {
        [SerializeField]
        private GoreEffectResource _effectResource;

        [SerializeField]
        private float _wallScanDistance;

        [SerializeField]
        private float _groundScanDistance;

        [SerializeField]
        private float _groundOffsetMin, _groundOffsetMax;

        [SerializeField]
        [Range(0f, 1f)]
        private float _hitSplatterProbability;

        private Entity _entity;

        private void OnEnable()
        {
            _entity = GetComponent<Entity>();
            _entity.TookDamage += OnTookDamage;
        }

        private void OnDisable()
        {
            _entity.TookDamage -= OnTookDamage;
        }

        private void OnTookDamage(in ImpactContext context)
        {
            if (_effectResource == null)
            {
                return;
            }
            if (context.IsFatal)
            {
                if (!PlaceDecalOnWall(context, _effectResource.KillSplattersWall.Sample()))
                {
                    PlaceDecalOnGround(context, _effectResource.KillSplattersGround.Sample(), project: false);
                }
            }
            else
            {
                if (!PlaceDecalOnWall(context, _effectResource.HitSplattersWall.Sample()))
                {
                    PlaceDecalOnGround(context, _effectResource.HitSplattersGround.Sample(), project: true);
                }
            }
        }

        private bool PlaceDecalOnWall(in ImpactContext context, DecalDescriptor decal)
        {
            if (decal != null && Physics.Raycast(
                context.ContactPoint,
                context.ContactDirection,
                out RaycastHit hitInfo,
                _wallScanDistance,
                ActorLayers.WallMask,
                QueryTriggerInteraction.Ignore) &&
                PrefabRegistry.Rent(decal, out DecalDescriptor splatter))
            {
                splatter.PlaceNormal(hitInfo.point, hitInfo.normal);
                return true;
            }
            return false;
        }

        private bool PlaceDecalOnGround(in ImpactContext context, DecalDescriptor decal, bool project)
        {
            if (decal != null && Physics.Raycast(
                context.ContactPoint + context.ContactDirection * Random.Shared.NextSingle(_groundOffsetMin, _groundOffsetMax),
                Vector3.down,
                out RaycastHit hitInfo,
                _groundScanDistance,
                ActorLayers.GroundMask,
                QueryTriggerInteraction.Ignore) &&
                PrefabRegistry.Rent(decal, out DecalDescriptor splatter))
            {
                if (project)
                {
                    splatter.PlaceProjected(hitInfo.point, context.ContactDirection, hitInfo.normal);
                }
                else
                {
                    splatter.PlaceNormal(hitInfo.point, hitInfo.normal);
                }
                return true;
            }
            return false;
        }
    }
}