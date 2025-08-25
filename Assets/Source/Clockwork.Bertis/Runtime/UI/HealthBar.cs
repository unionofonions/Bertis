using Clockwork.Bertis.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Clockwork.Bertis.UI
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField]
        private Image _foregroundImage;

        [SerializeField]
        private Image _trailImage;

        [SerializeField]
        private float _trailDuration;

        [SerializeField]
        private AnimationCurve _trailInterp;

        private float _trailProgress;
        private float _trailSource;
        private float _trailTarget;

        [SerializeField]
        private Entity _entity;

        protected void Awake()
        {
            if (_entity != null)
            {
                float healthRatio = _entity.HealthRatio;

                if (_foregroundImage != null)
                {
                    _foregroundImage.fillAmount = healthRatio;
                }

                if (_trailImage != null)
                {
                    _trailImage.gameObject.SetActive(false);
                    _trailImage.fillAmount = healthRatio;
                }

                _entity.HealthChanged += OnEntityHealthChanged;
            }

            enabled = false;
        }

        protected void OnDestroy()
        {
            if (_entity != null)
            {
                _entity.HealthChanged -= OnEntityHealthChanged;
            }
        }

        protected void Update()
        {
            if (_trailImage == null)
            {
                return;
            }

            _trailProgress += Time.unscaledDeltaTime / _trailDuration;

            if (_trailProgress >= 1f)
            {
                _trailImage.gameObject.SetActive(false);
                enabled = false;
                return;
            }

            float evaluation = _trailInterp.Evaluate(_trailProgress);
            _trailImage.fillAmount = Math.Lerp(_trailSource, _trailTarget, evaluation);
        }

        private void OnEntityHealthChanged(Entity entity)
        {
            if (_foregroundImage != null)
            {
                float prevRatio = _foregroundImage.fillAmount;
                float nextRatio = _entity.HealthRatio;

                _foregroundImage.fillAmount = nextRatio;

                if (nextRatio < prevRatio && _trailImage != null)
                {
                    _trailProgress = 0f;
                    _trailSource = enabled ? _trailImage.fillAmount : prevRatio;
                    _trailTarget = nextRatio;
                    _trailImage.gameObject.SetActive(true);
                    enabled = true;
                }
            }
        }
    }
}