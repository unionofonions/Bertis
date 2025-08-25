using UnityEngine;
using Clockwork.Bertis.Gameplay;

namespace Clockwork.Bertis.UI
{
    [RequireComponent(typeof(Entity))]
    public sealed class MinimapElement : MonoBehaviour
    {
        [SerializeField]
        private GameObject _iconRenderer;

        private Entity _entity;

        private void Awake()
        {
            _entity = GetComponent<Entity>();
            if (_entity.HasHealth)
            {
                _entity.TookDamage += OnTookDamage;
                _entity.Revived += OnRevived;
            }
        }

        private void OnDestroy()
        {
            if (_entity.HasHealth)
            {
                _entity.TookDamage -= OnTookDamage;
                _entity.Revived -= OnRevived;
            }
        }

        private void OnTookDamage(in ImpactContext context)
        {
            if (context.IsFatal && _iconRenderer != null)
            {
                _iconRenderer.SetActive(false);
            }
        }

        private void OnRevived()
        {
            if (_iconRenderer != null)
            {
                _iconRenderer.SetActive(true);
            }
        }
    }
}