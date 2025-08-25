using UnityEngine;
using UnityEngine.Rendering;

namespace Clockwork.Simulation
{
    public class DynamicVolumeSystem : MonoBehaviour
    {
        [SerializeField]
        private FadeConfig _fadeConfig;
        private FadeCompositor _fadeCompositor;

        [SerializeField]
        private AnimationCurve _interpolation;

        private Volume _volume;

        public void Apply()
        {
            _fadeCompositor.Start();
        }

        private void Awake()
        {
            _volume = GetComponent<Volume>();
            _fadeCompositor = new FadeCompositor(SetFadeFactor, _fadeConfig);
        }

        private void Update()
        {
            _fadeCompositor.Update(Time.deltaTime);
        }

        private void OnValidate()
        {
            if (_fadeCompositor != null)
            {
                _fadeCompositor.Config = _fadeConfig;
            }
        }

        private void SetFadeFactor(float value)
        {
            _volume.weight = _interpolation.Evaluate(value);
        }
    }
}