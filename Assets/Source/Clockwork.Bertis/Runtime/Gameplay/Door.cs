using UnityEngine;
using Clockwork.Simulation;

namespace Clockwork.Bertis.Gameplay
{
    public class Door : Interactable
    {
        [SerializeField]
        private float _animationDuration;

        [SerializeField]
        private AnimationCurve _animationInterp;

        [SerializeField]
        private Quaternion _openRotation;

        private Quaternion _closeRotation;

        [SerializeField]
        private Transform _frameTransform;

        [SerializeField]
        private bool _allowToggle;

        private bool _isOpen;

        [SerializeField]
        private SoundDescriptor _openSound;

        [SerializeField]
        private SoundDescriptor _closeSound;

        protected override string Label => _isOpen ? "Close" : "Open";

        protected new void Awake()
        {
            base.Awake();
            _closeRotation = _frameTransform.localRotation;
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            _ = OpenCloseAsync(open: !_isOpen);
            SoundOutputModel.Play(_isOpen ? _closeSound : _openSound);
        }

        private async Awaitable OpenCloseAsync(bool open)
        {
            Quaternion sourceRotation = open ? _closeRotation : _openRotation;
            Quaternion targetRotation = open ? _openRotation : _closeRotation;

            float progress = 0f;
            do
            {
                progress += Time.deltaTime / _animationDuration;

                _frameTransform.localRotation = Math.Slerp(
                    sourceRotation,
                    targetRotation,
                    _animationInterp.Evaluate(progress));

                await Awaitable.NextFrameAsync();
            }
            while (progress < 1f);

            _frameTransform.localRotation = targetRotation;
            _isOpen = !_isOpen;

            if (_allowToggle)
            {
                Reactivate();
            }
        }

        [ContextMenu("Close Door")]
        private void CloseDoor()
        {
            _frameTransform.localRotation = _closeRotation;
            Reactivate();
        }
    }
}