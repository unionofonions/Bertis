using UnityEngine;
using UnityEngine.UI;
using Clockwork.Bertis.Gameplay;

namespace Clockwork.Bertis.Components
{
    public sealed class HitmarkerComponent : MonoBehaviour
    {
        [SerializeField]
        private HitmarkerDescriptor _hitDescriptor;
        [SerializeField]
        private HitmarkerDescriptor _killDescriptor;

        private HitmarkerDescriptor _currentDescriptor;
        private float _progress = 1f;

        [SerializeField]
        private Transform _indicatorParent;
        [SerializeField]
        private Image[] _indicatorImages;

        private void OnEnable()
        {
            _indicatorParent.localScale = Vector3.zero;
            Entity.EntityTookDamage += OnEntityTookDamage;
        }

        private void OnDisable()
        {
            Entity.EntityTookDamage -= OnEntityTookDamage;
        }

        private void Update()
        {
            if (_progress >= 1f || _indicatorParent == null)
            {
                return;
            }

            _progress = Math.Min(1f, _progress + Time.deltaTime / _currentDescriptor.Duration);

            float scale = _currentDescriptor.SizeScale * _currentDescriptor.SizeInterpolation.Evaluate(_progress);
            _indicatorParent.localScale = new Vector3(scale, scale);

            if (_currentDescriptor.RotateToIdentity)
            {
                float angle = Mathf.LerpAngle(_indicatorParent.localEulerAngles.z, 0f, _progress);
                _indicatorParent.localRotation = Math.FromEuler(new Vector3(0f, 0f, angle));
            }

            if (_progress >= 1f)
            {
                _currentDescriptor = null;
                _indicatorParent.localScale = Vector3.zero;
            }
        }

        private void OnEntityTookDamage(in ImpactContext context)
        {
            if (context.Instigator is not PlayerPuppet || context.Victim == null || !context.Victim.HasHealth)
            {
                return;
            }

            HitmarkerDescriptor descriptor = context.IsFatal ? _killDescriptor : _hitDescriptor;
            if (descriptor == null || _indicatorParent == null)
            {
                return;
            }

            Color color = descriptor.HealthToColor.Evaluate(context.Victim.HealthRatio);
            foreach (Image indicator in _indicatorImages)
            {
                if (indicator != null)
                {
                    indicator.color = color;
                }
            }

            float angle = !descriptor.RandomizeRotation
                ? descriptor.StartRotation
                : Random.Shared.NextSingle(descriptor.StartRotation);

            _currentDescriptor = descriptor;
            _progress = 0f;
            _indicatorParent.eulerAngles = new Vector3(0f, 0f, angle);
        }
    }
}