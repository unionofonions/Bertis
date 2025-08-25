using UnityEngine;
using UnityEngine.InputSystem;

namespace Clockwork.Bertis.UI
{
    public class MinimapSystem : MonoBehaviour
    {
        [SerializeField]
        private float _cameraSizeMin, _cameraSizeMax;

        [SerializeField]
        private float _cameraSizeDelta;

        [SerializeField]
        private Transform _followTransform;

        [SerializeField]
        private Camera _minimapCamera;

        [SerializeField]
        private InputActionReference _zoomInActionReference;
        [SerializeField]
        private InputActionReference _zoomOutActionReference;

        private void Awake()
        {
            if (_zoomInActionReference != null)
            {
                _zoomInActionReference.action.performed += OnZoomInActionPerformed;
                _zoomOutActionReference.action.performed += OnZoomOutActionPerformed;
            }
        }

        private void OnDestroy()
        {
            if (_zoomInActionReference != null)
            {
                _zoomInActionReference.action.performed -= OnZoomInActionPerformed;
                _zoomOutActionReference.action.performed -= OnZoomOutActionPerformed;
            }
        }

        private void FixedUpdate()
        {
            if (_followTransform != null)
            {
                transform.position = _followTransform.position;
            }
        }

        private void OnZoomInActionPerformed(InputAction.CallbackContext context)
            => SetCameraSize(1f);

        private void OnZoomOutActionPerformed(InputAction.CallbackContext context)
            => SetCameraSize(-1f);

        private void SetCameraSize(float sign)
        {
            if (_minimapCamera != null)
            {
                float size = _minimapCamera.orthographicSize;
                _minimapCamera.orthographicSize = Math.Clamp(size + sign * _cameraSizeDelta, _cameraSizeMin, _cameraSizeMax);
            }
        }
    }
}