using UnityEngine;
using Clockwork.Bertis.Gameplay;

namespace Clockwork.Bertis.Components
{
    [RequireComponent(typeof(Firearm))]
    public sealed class FirearmLaserSight : MonoBehaviour
    {
        [SerializeField]
        private LineRenderer _lineRenderer;

        private Firearm _firearm;

        private void OnEnable()
        {
            _firearm = GetComponent<Firearm>();
            _firearm.Reloaded += OnReloaded;
            _firearm.ReloadStarted += OnReloadStarted;
        }

        private void OnDisable()
        {
            _firearm.Reloaded -= OnReloaded;
            _firearm.ReloadStarted -= OnReloadStarted;
        }

        private void FixedUpdate()
        {
            if (_lineRenderer == null)
            {
                return;
            }

            _lineRenderer.transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
            float distance;

            if (Physics.Raycast(
                position,
                rotation * Vector3.forward,
                out RaycastHit hitInfo,
                _firearm.ProjectileMaxRange,
                ActorLayers.WallMask | ActorLayers.EnemyMask,
                QueryTriggerInteraction.Ignore))
            {
                distance = hitInfo.distance;
            }
            else
            {
                distance = _firearm.ProjectileMaxRange;
            }

            _lineRenderer.SetPosition(1, Vector3.forward * distance);
        }

        private void OnReloaded(Firearm firearm)
        {
            if (_lineRenderer != null)
            {
                _lineRenderer.enabled = true;
            }
        }

        private void OnReloadStarted(Firearm firearm)
        {
            if (_lineRenderer != null)
            {
                _lineRenderer.enabled = false;
            }
        }
    }
}