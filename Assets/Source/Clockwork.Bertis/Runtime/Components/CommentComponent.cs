using UnityEngine;
using Clockwork.Bertis.Gameplay;
using Clockwork.Bertis.UI;
using Clockwork.Collections;
using Clockwork.Pooling;

namespace Clockwork.Bertis.Components
{
    [RequireComponent(typeof(Entity))]
    public sealed class CommentComponent : MonoBehaviour
    {
        [SerializeField]
        [PrefabReference]
        private SubtitleElement _subtitleElement;

        [SerializeField]
        private Vector3 _followOffset;

        [SerializeField]
        private float _applyInterval;
        private float _applyTimestamp = float.NegativeInfinity;

        [SerializeField]
        private UniqueSampler<string> _nonFatalTexts;
        [SerializeField]
        private UniqueSampler<string> _fatalTexts;
        [SerializeField]
        private UniqueSampler<string> _enteredCombatTexts;
        [SerializeField]
        private UniqueSampler<string> _exitedCombatTexts;
        [SerializeField]
        private UniqueSampler<string> _nonFatalExplosiveTexts;
        [SerializeField]
        private UniqueSampler<string> _fatalExplosiveTexts;

        private Entity _entity;

        private void Awake()
        {
            _entity = GetComponent<Entity>();

            _entity.TookDamage += OnTookDamage;
            if (_entity is EscortPuppet escortPuppet)
            {
                escortPuppet.CombatStatusChanged += OnCombatStatusChanged;
            }
        }

        private void OnDestroy()
        {
            _entity.TookDamage -= OnTookDamage;
            if (_entity is EscortPuppet escortPuppet)
            {
                escortPuppet.CombatStatusChanged -= OnCombatStatusChanged;
            }
        }

        private void OnTookDamage(in ImpactContext context)
        {
            PopUpText(context.IsFatal ? _fatalTexts : _nonFatalTexts);
        }

        private void OnCombatStatusChanged(bool enteredCombat)
        {
            PopUpText(enteredCombat ? _enteredCombatTexts : _exitedCombatTexts);
        }

        private void PopUpText(UniqueSampler<string> sampler)
        {
            if ((Time.time - _applyTimestamp) < _applyInterval)
            {
                return;
            }
            _applyTimestamp = Time.time;

            if (PrefabRegistry.Rent(_subtitleElement, out SubtitleElement subtitleElement))
            {
                subtitleElement.SetText(sampler.Sample(), transform, _followOffset);
            }
        }
    }
}