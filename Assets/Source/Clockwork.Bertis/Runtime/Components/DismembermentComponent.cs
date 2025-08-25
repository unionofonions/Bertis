using System;
using UnityEngine;
using Clockwork.Bertis.Gameplay;
using Clockwork.Collections;
using Clockwork.Pooling;

namespace Clockwork.Bertis.Components
{
    [RequireComponent(typeof(Entity))]
    public sealed class DismembermentComponent : MonoBehaviour
    {
        [SerializeField]
        private float _forceMin, _forceMax;
        [SerializeField]
        private Vector3 _torqueMax;
        [SerializeField]
        private float _rotationMax;

        [SerializeField]
        private AnimationCurve _explosionProbability;

        [SerializeField]
        private UniqueSampler<Limb> _limbs;
        private readonly Vector<Limb> _activeLimbs = new();

        private Entity _entity;

        public void Dismember(Vector3 direction)
        {
            for (int i = 0; i < _limbs.Count && Random.Shared.NextBoolean(
                _explosionProbability.Evaluate(i)); i++)
            {
                Limb limb = _limbs.Sample();
                limb.Explode(this, direction);
                _activeLimbs.Push(limb);
            }
        }

        public void ReverseDismember()
        {
            while (_activeLimbs.TryPop(out Limb limb))
            {
                limb.Reset();
            }
            _limbs.Reset();
        }

        private void OnEnable()
        {
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
                Dismember(context.ContactDirection);
            }
        }

        private void OnRevived()
        {
            ReverseDismember();
        }

        [Serializable]
        private class Limb
        {
            [PrefabReference]
            public DismembermentDebris Debris;
            public GameObject Mesh;
            public Transform Bone;

            public void Explode(DismembermentComponent component, Vector3 direction)
            {
                Mesh.SetActive(false);

                if (PrefabRegistry.Rent(Debris, out DismembermentDebris debris))
                {
                    float angle = Random.Shared.NextSingle(component._rotationMax);

                    Vector3 force = Quaternion.AngleAxis(angle, Vector3.up)
                        * new Vector3(direction.x, 0.3f, direction.z)
                        * Random.Shared.NextSingle(component._forceMin, component._forceMax);

                    Vector3 torque = Random.Shared.NextVector3(component._torqueMax);
                    debris.Spawn(Bone.position, force, torque);
                }
            }

            public void Reset()
            {
                Mesh.SetActive(true);
            }
        }
    }
}