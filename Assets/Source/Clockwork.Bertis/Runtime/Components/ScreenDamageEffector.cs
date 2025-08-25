using Clockwork.Bertis.Gameplay;
using Clockwork.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace Clockwork.Bertis.Components
{
    public sealed class ScreenDamageEffector : MonoBehaviour
    {
        [SerializeField]
        private FadeConfig _fadeConfig;
        private FadeCompositor _fadeCompositor;

        [SerializeField]
        private Image _image;

        void SetFadeFactor(float value)
        {
            Color color = _image.color;
            color.a = value;
            _image.color = color;
        }

        private void Awake()
        {
            Entity.EntityTookDamage += OnEntityTookDamage;

            _fadeCompositor = new FadeCompositor(
                SetFadeFactor,
                _fadeConfig);

            enabled = false;
        }

        private void OnDestroy()
        {
            Entity.EntityTookDamage -= OnEntityTookDamage;
            _fadeCompositor = null;
        }

        private void Update()
        {
            if (_image == null)
            {
                return;
            }

            if (!_fadeCompositor.Update(Time.deltaTime))
            {
                _image.gameObject.SetActive(false);
                enabled = false;
            }
        }

        private void OnEntityTookDamage(in ImpactContext context)
        {
            if (context.IsVictimHurt && context.Victim is PlayerPuppet &&
                _image != null && _fadeCompositor.Start())
            {
                _image.gameObject.SetActive(true);
                enabled = true;
            }
        }
    }
}