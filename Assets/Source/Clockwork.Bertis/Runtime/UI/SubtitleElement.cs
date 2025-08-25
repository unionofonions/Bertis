using UnityEngine;
using Clockwork.Pooling;
using Clockwork.Simulation;
using Label = TMPro.TextMeshProUGUI;

namespace Clockwork.Bertis.UI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class SubtitleElement : MonoBehaviour
    {
        [SerializeField]
        private FadeConfig _fadeConfig;
        private FadeCompositor _fadeCompositor;

        [SerializeField]
        private Label _label;

        private Vector3 _followOffset;
        private Transform _followTransform;

        private CanvasGroup _canvasGroup;

        public void SetText(string value, Transform followTransform, Vector3 followOffset)
        {
            if (followTransform == null || _label == null || !_fadeCompositor.Start())
            {
                Deactivate();
                return;
            }

            _label.text = value;
            _followOffset = followOffset;
            _followTransform = followTransform;
            gameObject.SetActive(true);
        }

        private void Awake()
        {
            _fadeCompositor = new FadeCompositor(value => { _canvasGroup.alpha = value; }, _fadeConfig);
            _canvasGroup = GetComponent<CanvasGroup>();
            GetComponent<Canvas>().worldCamera = Camera.main;
        }

        private void Update()
        {
            if (!_fadeCompositor.Update(Time.deltaTime))
            {
                Deactivate();
                return;
            }

            if (_followTransform != null)
            {
                transform.position = _followTransform.position + _followOffset;
            }
        }

        private void Deactivate()
        {
            gameObject.SetActive(false);
            PrefabRegistry.Return(this);
        }
    }
}