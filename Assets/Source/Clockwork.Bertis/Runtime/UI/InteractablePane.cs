using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Label = TMPro.TextMeshProUGUI;

namespace Clockwork.Bertis.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class InteractablePane : MonoBehaviour
    {
        [SerializeField]
        private Label _frontLabel;

        [SerializeField]
        private Label _behindLabel;

        [SerializeField]
        private RectTransform _control;

        [SerializeField]
        private LineRenderer _line;

        [SerializeField]
        private Transform _circle;

        [SerializeField]
        private Image _progressFill;

        [SerializeField]
        private RectMask2D _progressMask;

        private float _progressMaskWidth;

        private float _fadeFactor = 1f;

        [SerializeField]
        private Vector3 _followOffset;

        private Transform _followTransform;
        private Transform _targetTransform;

        private Camera _camera;
        private CanvasGroup _canvasGroup;

        [SerializeField]
        private InputActionReference _activateActionReference;

        private bool _isActivating;
        private float _activationProgress;
        private float _activationDeltaScale;

        public event Action Activated;

        public float FadeFactor
        {
            set
            {
                value = Math.Clamp01(value);
                if (value != _fadeFactor)
                {
                    _fadeFactor = value;
                    _canvasGroup.alpha = value;
                    _line.material.color = new Color(1f, 1f, 1f, value);
                }
            }
        }

        private float ActivationProgress
        {
            get => _activationProgress;
            set
            {
                value = Math.Clamp01(value);
                if (value != _activationProgress)
                {
                    _activationProgress = value;
                    _progressFill.fillAmount = value;
                    _progressMask.padding = new Vector4(value * _progressMaskWidth, 0f);
                }
            }
        }

        public void Attach(
            string label,
            float activationDuration,
            Transform targetTransform,
            Transform followTransform)
        {
            _targetTransform = targetTransform;
            _followTransform = followTransform;
            _activationDeltaScale = 1f / activationDuration;
            _line.SetPosition(0, targetTransform.position);
            _frontLabel.text = label;
            _behindLabel.text = label;
            ActivationProgress = 0f;
        }

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _camera = Camera.main;
            _progressMaskWidth = _progressMask.rectTransform.rect.width;
        }

        private void OnEnable()
        {
            if (_activateActionReference != null)
            {
                _activateActionReference.action.started += OnActivateActionStateChanged;
                _activateActionReference.action.canceled += OnActivateActionStateChanged;
            }
        }

        private void OnDisable()
        {
            if (_activateActionReference != null)
            {
                _activateActionReference.action.started -= OnActivateActionStateChanged;
                _activateActionReference.action.canceled -= OnActivateActionStateChanged;
            }
        }

        private void LateUpdate()
        {
            if (_isActivating)
            {
                ActivationProgress += Time.deltaTime * _activationDeltaScale;

                if (_activationProgress >= 1f)
                {
                    _isActivating = false;
                    Activated?.Invoke();
                }
            }

            if (_targetTransform != null && _followTransform != null)
            {
                Vector3 endPoint = _control.position = _camera.WorldToScreenPoint(_followTransform.position + _followOffset);
                endPoint.z = _camera.nearClipPlane;
                _line.SetPosition(1, _camera.ScreenToWorldPoint(endPoint));
                _circle.position = _camera.WorldToScreenPoint(_targetTransform.position);
            }
        }

        private void OnActivateActionStateChanged(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    _isActivating = true;
                    break;
                case InputActionPhase.Canceled:
                    _isActivating = false;
                    ActivationProgress = 0f;
                    break;
            }
        }
    }
}