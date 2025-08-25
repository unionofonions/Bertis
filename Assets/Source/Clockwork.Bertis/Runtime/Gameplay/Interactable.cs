using UnityEngine;
using UnityEngine.Events;
using Clockwork.Bertis.UI;
using Clockwork.Pooling;

namespace Clockwork.Bertis.Gameplay
{
    [RequireComponent(typeof(SphereCollider))]
    public class Interactable : MonoBehaviour
    {
        [SerializeField]
        [PrefabReference]
        private InteractablePane _panePrefab;

        private InteractablePane _rentedPane;

        [SerializeField]
        private string _defaultLabel;

        [SerializeField]
        private float _activationDuration;

        [SerializeField]
        private Transform _targetTransform;

        [SerializeField]
        private Transform _followTransform;

        [SerializeField]
        private AnimationCurve _distanceToVisibility;

        private SphereCollider _collider;

        [SerializeField]
        private UnityEvent _activated;

        protected virtual string Label => _defaultLabel;

        protected void Reactivate()
            => _collider.enabled = true;

        protected void Awake()
        {
            _collider = GetComponent<SphereCollider>();
        }

        protected void OnTriggerEnter(Collider other)
        {
            if (PrefabRegistry.Rent(_panePrefab, out _rentedPane))
            {
                _rentedPane.Attach(
                    Label,
                    _activationDuration,
                    _targetTransform,
                    _followTransform);

                _rentedPane.gameObject.SetActive(true);
                _rentedPane.Activated += OnActivated;
            }
        }

        protected void OnTriggerExit(Collider other)
        {
            ReturnRentedPane();
        }

        protected void OnTriggerStay(Collider other)
        {
            if (_rentedPane != null)
            {
                float distance = Math.Distance(transform.position, other.transform.position);
                _rentedPane.FadeFactor = _distanceToVisibility.Evaluate(distance / _collider.radius);
            }
        }

        protected virtual void OnActivated()
        {
            Debug.LogInformation($"{name} activated.");
            _collider.enabled = false;
            ReturnRentedPane();
            _activated.Invoke();
        }

        private void ReturnRentedPane()
        {
            _rentedPane.Activated -= OnActivated;
            _rentedPane.gameObject.SetActive(false);
            PrefabRegistry.Return(_rentedPane);
        }
    }
}