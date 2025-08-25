using System;
using Clockwork.Bertis.Gameplay;
using UnityEngine;

namespace Clockwork.Bertis.Components
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Entity))]
    public sealed class RagdollComponent : MonoBehaviour
    {
        [SerializeField]
        private float _horForceMin, _horForceMax;
        [SerializeField]
        private float _verForceMin, _verForceMax;

        [SerializeField]
        private Limb[] _limbs;

        private Collider _collider;
        private Animator _animator;
        private Entity _entity;

        public void EnableRagdoll(Vector3 forceDirection)
        {
            ToggleRagdoll(true);

            float horForce = Random.Shared.NextSingle(_horForceMin, _horForceMax);
            float verForce = Random.Shared.NextSingle(_verForceMin, _verForceMax);
            Vector3 force = new Vector3(forceDirection.x * horForce, verForce, forceDirection.z * horForce);

            foreach (Limb limb in _limbs)
            {
                limb.ToggleRagdoll(true);
                limb.AddForce(force);
            }
        }

        public void DisableRagdoll()
        {
            ToggleRagdoll(false);

            foreach (Limb limb in _limbs)
            {
                limb.ToggleRagdoll(false);
            }
        }

        private void ToggleRagdoll(bool enable)
        {
            _collider.enabled = !enable;
            _animator.enabled = !enable;
        }

        private void OnEnable()
        {
            _collider = GetComponent<Collider>();
            _animator = GetComponent<Animator>();
            _entity = GetComponent<Entity>();

            _entity.TookDamage += OnTookDamage;
            _entity.Revived += OnRevived;
        }

        private void OnDisable()
        {
            _entity.TookDamage -= OnTookDamage;
            _entity.Revived -= OnRevived;
        }

        private void OnTookDamage(in ImpactContext context)
        {
            if (context.IsFatal)
            {
                EnableRagdoll(context.ContactDirection);
            }
        }

        private void OnRevived()
        {
            DisableRagdoll();
        }

        [ContextMenu("Set-up limbs")]
        private void SetUpLimbs()
        {
            Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
            _limbs = new Limb[rigidbodies.Length];

            for (int i = 0; i < _limbs.Length; i++)
            {
                Rigidbody rigidbody = rigidbodies[i];
                Limb limb = _limbs[i] = new Limb();

                limb.Rigidbody = rigidbody;
                limb.Collider = rigidbody.GetComponent<Collider>();
            }
        }

        [Serializable]
        private class Limb
        {
            public Rigidbody Rigidbody;
            public Collider Collider;

            public void ToggleRagdoll(bool enable)
            {
                Rigidbody.isKinematic = !enable;
                Collider.enabled = enable;

                if (enable)
                {
                    Rigidbody.linearVelocity = Vector3.zero;
                    Rigidbody.angularVelocity = Vector3.zero;
                }
            }

            public void AddForce(Vector3 force)
            {
                Rigidbody.AddForce(force, ForceMode.Impulse);
            }
        }
    }
}