using UnityEngine;
using Clockwork.Bertis.Gameplay;
using Label = TMPro.TextMeshProUGUI;

namespace Clockwork.Bertis.UI
{
    public sealed class PlayerBulletCounter : MonoBehaviour
    {
        [SerializeField]
        private Label _actualLabel;
        [SerializeField]
        private Label _shadowLabel;

        [SerializeField]
        private AnimationCurve _positionInterpolation;
        [SerializeField]
        private AnimationCurve _alphaInterpolation;

        [SerializeField]
        private float _animationDuration;
        private float _animationProgress = 1f;

        [SerializeField]
        private PlayerPuppet _playerPuppet;

        private void Start()
        {
            if (_playerPuppet != null && _playerPuppet.Firearm != null)
            {
                _playerPuppet.Firearm.FiredRound += OnFiredRound;
                _playerPuppet.Firearm.Reloaded += OnReloaded;

                UpdateActualLabel(_playerPuppet.Firearm);
                if (_shadowLabel != null)
                {
                    _shadowLabel.alpha = 0f;
                }
            }
        }

        private void OnDestroy()
        {
            if (_playerPuppet != null && _playerPuppet.Firearm != null)
            {
                _playerPuppet.Firearm.FiredRound -= OnFiredRound;
                _playerPuppet.Firearm.Reloaded -= OnReloaded;
            }
        }

        private void Update()
        {
            if (_animationProgress < 1f && _shadowLabel != null)
            {
                _animationProgress += Time.deltaTime / _animationDuration;
                if (_animationProgress < 1f)
                {
                    _shadowLabel.rectTransform.anchoredPosition = _positionInterpolation.Evaluate(_animationProgress) * Vector2.up;
                    _shadowLabel.alpha = _alphaInterpolation.Evaluate(_animationProgress);
                }
                else
                {
                    _animationProgress = 1f;
                    _shadowLabel.alpha = 0f;
                }
            }
        }

        private void OnValidate()
        {
            _animationDuration = Math.Max(0f, _animationDuration);
        }

        private void OnFiredRound(Firearm firearm)
        {
            UpdateActualLabel(firearm);
            UpdateShadowLabel(firearm);
        }

        private void OnReloaded(Firearm firearm)
        {
            UpdateActualLabel(firearm);
        }

        private void UpdateActualLabel(Firearm firearm)
        {
            if (_actualLabel != null)
            {
                _actualLabel.SetText("{0}", firearm.RoundsInMagazine);
            }
        }

        private void UpdateShadowLabel(Firearm firearm)
        {
            if (_shadowLabel != null)
            {
                _shadowLabel.SetText("{0}", firearm.RoundsInMagazine + 1);
                _animationProgress = 0f;
            }
        }
    }
}