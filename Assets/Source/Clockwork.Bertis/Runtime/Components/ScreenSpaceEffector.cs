using UnityEngine;
using UnityEngine.UI;
using Clockwork.Bertis.Gameplay;
using Clockwork.Simulation;

namespace Clockwork.Bertis.Components
{
    public sealed class ScreenSpaceEffector : MonoBehaviour
    {
        [SerializeField]
        private FadeConfig _fadeConfig;

        private FadeCompositor[] _fadeCompositors;

        [SerializeField]
        private float _applyDistance;

        [SerializeField]
        [Range(0f, 1f)]
        private float _applyProbability;

        [SerializeField]
        private Image[] _layers;

        private int _nextLayer;

        private void Awake()
        {
            Entity.EntityTookDamage += OnEntityTookDamage;

            _fadeCompositors = new FadeCompositor[_layers.Length];
            for (int i = 0; i < _fadeCompositors.Length; i++)
            {
                Image layer = _layers[i];
                _fadeCompositors[i] = new FadeCompositor(
                    value => SetFadeFactor(layer, value),
                    _fadeConfig);
            }

            static void SetFadeFactor(Image layer, float value)
            {
                Color color = layer.color;
                color.a = value;
                layer.color = color;
            }
        }

        private void OnDestroy()
        {
            Entity.EntityTookDamage -= OnEntityTookDamage;
            _fadeCompositors = null;
        }

        private void Update()
        {
            bool disable = true;

            for (int i = 0; i < _fadeCompositors.Length; i++)
            {
                FadeCompositor fadeCompositor = _fadeCompositors[i];
                if (fadeCompositor.State != FadeState.None)
                {
                    if (!fadeCompositor.Update(Time.deltaTime))
                    {
                        _layers[i].gameObject.SetActive(false);
                    }
                    else
                    {
                        disable = false;
                    }
                }
            }

            if (disable)
            {
                enabled = false;
            }
        }

        private void OnValidate()
        {
            if (_fadeCompositors != null)
            {
                foreach (FadeCompositor fadeCompositor in _fadeCompositors)
                {
                    fadeCompositor.Config = _fadeConfig;
                }
            }
        }

        private void OnEntityTookDamage(in ImpactContext context)
        {
            if (context.IsVictimHurt && context.Victim.Family == EntityFamily.Zombie &&
                Math.Nearby(context.Instigator.transform.position, context.Victim.transform.position, _applyDistance) &&
                (context.IsFatal || Random.Shared.NextBoolean(_applyProbability)))
            {
                _nextLayer = _nextLayer >= _layers.Length - 1 ? 0 : _nextLayer + 1;
                _fadeCompositors[_nextLayer].Start();
                _layers[_nextLayer].gameObject.SetActive(true);
                enabled = true;
            }
        }
    }
}