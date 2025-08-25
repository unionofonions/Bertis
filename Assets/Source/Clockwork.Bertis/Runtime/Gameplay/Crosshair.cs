using UnityEngine;
using UnityEngine.InputSystem;

namespace Clockwork.Bertis.Gameplay
{
    public sealed class Crosshair : MonoBehaviour
    {
        [SerializeField]
        private Vector2 _lookSensitivity;

        private Vector2 _screenPosition;
        private Vector2 _boundsMin;
        private Vector2 _boundsMax;

        [SerializeField]
        private Camera _camera;

        [SerializeField]
        private AnimationCurve _spreadToDistance;
        [SerializeField]
        private RectTransform[] _spreadIndicators;

        [SerializeField]
        private InputActionReference _lookActionReference;

        public Vector2 ScreenPosition
        {
            get => _screenPosition;
            set
            {
                transform.position = _screenPosition = Math.Clamp(value, _boundsMin, _boundsMax);
            }
        }

        private Vector2 ScreenSize
        {
            get
            {
                Resolution resolution = Screen.currentResolution;
                return new(resolution.width, resolution.height);
            }
        }

        public float SpreadFactor
        {
            set
            {
                float distance = Math.Max(0.1f, _spreadToDistance.Evaluate(value));
                foreach (RectTransform indicator in _spreadIndicators)
                {
                    if (indicator != null)
                    {
                        indicator.anchoredPosition = distance * Math.Sign(indicator.anchoredPosition);
                    }
                }
            }
        }

        public (Quaternion Rotation, float AimOffset) GetActorLookRotation(Vector3 actorPosition, Quaternion actorRotation, float verticalOffset)
        {
            if (_camera != null)
            {
                Ray ray = _camera.ScreenPointToRay(_screenPosition);
                if (new Plane(Vector3.up, actorPosition + verticalOffset * Vector3.up).Raycast(ray, out float distance))
                {
                    Vector3 direction = ray.GetPoint(distance) - actorPosition;
                    direction.y = 0f;

                    return (
                        Quaternion.LookRotation(direction),
                        Math.Dot(direction, actorRotation * Vector3.forward));
                }
            }
            return (Quaternion.identity, 0f);
        }

        private void OnEnable()
        {
            RecalculateBounds();
            ScreenPosition = ScreenSize / 2f;

            if (_lookActionReference != null)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                _lookActionReference.action.performed += OnLookActionPerformed;
            }
        }

        private void OnDisable()
        {
            if (_lookActionReference != null)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                _lookActionReference.action.performed -= OnLookActionPerformed;
            }
        }

        private void RecalculateBounds()
        {
            Vector2 extents = ((RectTransform)transform).rect.size / 2f;
            _boundsMin = extents;
            _boundsMax = ScreenSize - extents;
        }

        private void OnLookActionPerformed(InputAction.CallbackContext context)
        {
            ScreenPosition += _lookSensitivity * context.ReadValue<Vector2>();
        }
    }
}