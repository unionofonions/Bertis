using UnityEngine;
using Clockwork.Pooling;
using Clockwork.Simulation;

namespace Clockwork.Bertis.Components
{
    public sealed class DismembermentDebris : MonoBehaviour, IPoolWorker
    {
        [SerializeField]
        private FadeConfig _fadeConfig;
        private FadeCompositor _fadeCompositor;

        [SerializeField]
        private Vector3[] _offsets;
        private Rigidbody[] _fragments;

        public void Spawn(Vector3 position, Vector3 force, Vector3 torque)
        {
            if (!_fadeCompositor.Start())
            {
                Deactivate();
                return;
            }

            transform.position = position;
            gameObject.SetActive(true);

            foreach (Rigidbody fragment in _fragments)
            {
                fragment.linearVelocity = force;
                fragment.angularVelocity = torque;
            }
        }

        private void Awake()
        {
            _fragments = GetComponentsInChildren<Rigidbody>();
            _fadeCompositor = new FadeCompositor(SetFadeFactor, _fadeConfig);
        }

        private void Update()
        {
            if (!_fadeCompositor.Update(Time.deltaTime))
            {
                Deactivate();
            }
        }

        private void SetFadeFactor(float value)
        {
            foreach (Rigidbody fragment in _fragments)
            {
                fragment.transform.localScale = value * Vector3.one;
            }
        }

        private void Deactivate()
        {
            gameObject.SetActive(false);

            for (int i = 0; i < _fragments.Length; i++)
            {
                Rigidbody fragment = _fragments[i];
                fragment.linearVelocity = Vector3.zero;
                fragment.transform.localPosition = _offsets[i];
            }
        }

        void IPoolWorker.ScheduleEarlyReturn()
        {
            if (_fadeCompositor.State != FadeState.FadeOut)
            {
                _fadeCompositor.Start(FadeState.FadeOut);
            }
        }

        [ContextMenu("Update Offsets")]
        private void UpdateOffsets()
        {
            _offsets = new Vector3[transform.childCount];
            for (int i = 0; i < _offsets.Length; i++)
            {
                _offsets[i] = transform.GetChild(i).localPosition;
            }
        }
    }
}