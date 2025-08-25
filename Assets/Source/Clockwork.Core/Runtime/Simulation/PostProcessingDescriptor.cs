using UnityEngine;
using UnityEngine.Rendering;

namespace Clockwork.Simulation
{
    [RequireComponent(typeof(Volume))]
    public sealed class PostProcessingDescriptor : MonoBehaviour
    {
        [SerializeField]
        private FadeConfig _fadeConfig;

        private FadeCompositor _fadeCompositor;

        [SerializeField]
        private AnimationCurve _fadeInterpolation;

        private Volume _volume;

        private void SetFadeFactor(float value)
        {
            _volume.weight = _fadeInterpolation.Evaluate(value);
        }

        public void StartAnimation()
        {
            if (_fadeCompositor.Start())
            {
                enabled = true;
            }
        }

        private void Awake()
        {
            _volume = GetComponent<Volume>();
            _fadeCompositor = new FadeCompositor(SetFadeFactor, _fadeConfig);
        }

        private void Update()
        {
            if (!_fadeCompositor.Update(Time.unscaledDeltaTime))
            {
                enabled = false;
            }
        }

        private void OnValidate()
        {
            if (_fadeCompositor != null)
            {
                _fadeCompositor.Config = _fadeConfig;
            }
        }
    }
}