using UnityEngine;
using Clockwork.Collections;

#nullable enable

namespace Clockwork.Bertis.Components
{
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(CameraFollowComponent))]
    public sealed class CameraBlendComponent : MonoBehaviour
    {
        [SerializeField]
        private CameraBlendDescriptor? _defaultDescriptor;
        private CameraBlendDescriptor? _sourceDescriptor;
        private CameraBlendDescriptor? _targetDescriptor;
        private readonly Vector<CameraBlendDescriptor> _pendingDescriptors = new(4);

        private float _blendProgress = 1f;

        private Camera? _camera;
        private CameraFollowComponent? _followComponent;

        public void QueueBlend(CameraBlendDescriptor? descriptor)
        {
            if (descriptor == null)
            {
                return;
            }

            if (_targetDescriptor == null)
            {
                PrepareBlend(descriptor);
            }
            else if (CompareDescriptors(descriptor, _targetDescriptor) > 0)
            {
                Debug.Assert(_defaultDescriptor != null);
                if (_targetDescriptor != _defaultDescriptor)
                {
                    _pendingDescriptors.Push(_targetDescriptor);
                }
                PrepareBlend(descriptor);
            }
            else if (descriptor != _targetDescriptor && !_pendingDescriptors.Contains(descriptor))
            {
                _pendingDescriptors.Push(descriptor);
            }
            else
            {
                Debug.LogInformation("QueueBlend failed: descriptor already queued.", context: descriptor);
            }
        }

        public void AbortBlend(CameraBlendDescriptor? descriptor)
        {
            if (descriptor == null)
            {
                return;
            }

            if (descriptor == _targetDescriptor)
            {
                if (!_pendingDescriptors.IsEmpty)
                {
                    PrepareBlend(PopMax(_pendingDescriptors));
                }
                else
                {
                    Debug.Assert(_defaultDescriptor != null);
                    PrepareBlend(_defaultDescriptor);
                }
            }
            else if (!_pendingDescriptors.SwapRemove(descriptor))
            {
                Debug.LogInformation("AbortBlend failed: descriptor not queued.", context: descriptor);
            }

                static CameraBlendDescriptor PopMax(Vector<CameraBlendDescriptor> vector)
                {
                    CameraBlendDescriptor result = vector[0];
                    int i = 0;

                    for (int j = 1; j < vector.Count; j++)
                    {
                        CameraBlendDescriptor item = vector[j];
                        if (CompareDescriptors(item, result) > 0)
                        {
                            result = item;
                            i = j;
                        }
                    }

                    vector.SwapRemoveAt(i);
                    return result;
                }
        }

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _followComponent = GetComponent<CameraFollowComponent>();
            _sourceDescriptor = ScriptableObject.CreateInstance<CameraBlendDescriptor>();
        }

        private void Start()
        {
            if (_defaultDescriptor != null)
            {
                OverwriteState(_defaultDescriptor);
            }
            else
            {
                _defaultDescriptor = ScriptableObject.CreateInstance<CameraBlendDescriptor>();
                _defaultDescriptor.BlendDuration = 0.5f;
                _defaultDescriptor.BlendInterpolation = AnimationCurve.Linear(0f, 0f, 1f, 1f);
                SnapshotState(_defaultDescriptor);
            }
            _defaultDescriptor.Priority = int.MinValue;
        }

        private void OnDestroy()
        {
            if (_sourceDescriptor != null)
            {
                Destroy(_sourceDescriptor);
            }
        }

        private void Update()
        {
            if (_blendProgress >= 1f)
            {
                return;
            }

            Debug.Assert(_camera != null);
            Debug.Assert(_followComponent != null);
            Debug.Assert(_sourceDescriptor != null);
            Debug.Assert(_targetDescriptor != null);

            _blendProgress = Math.Min(1f, _blendProgress + Time.deltaTime / _targetDescriptor.BlendDuration);
            float evaluation = Math.Clamp01(_targetDescriptor.BlendInterpolation.Evaluate(_blendProgress));

            _camera.fieldOfView = Math.Lerp(_sourceDescriptor.CameraFieldOfView, _targetDescriptor.CameraFieldOfView, evaluation);
            _followComponent.FollowOffset = Math.Lerp(_sourceDescriptor.FollowOffset, _targetDescriptor.FollowOffset, evaluation);
            _followComponent.FollowSmoothness = Math.Lerp(_sourceDescriptor.FollowSmoothness, _targetDescriptor.FollowSmoothness, evaluation);

            if (_blendProgress >= 1f && _targetDescriptor == _defaultDescriptor)
            {
                _targetDescriptor = null;
            }
        }

        private void PrepareBlend(CameraBlendDescriptor descriptor)
        {
            Debug.Assert(_sourceDescriptor != null);
            _blendProgress = 0f;
            _targetDescriptor = descriptor;
            SnapshotState(_sourceDescriptor);
        }

        private void OverwriteState(CameraBlendDescriptor descriptor)
        {
            Debug.Assert(_camera != null);
            Debug.Assert(_followComponent != null);
            _camera.fieldOfView = descriptor.CameraFieldOfView;
            _followComponent.FollowOffset = descriptor.FollowOffset;
            _followComponent.FollowSmoothness = descriptor.FollowSmoothness;
            _followComponent.SyncWithTarget();
        }

        private void SnapshotState(CameraBlendDescriptor descriptor)
        {
            Debug.Assert(_camera != null);
            Debug.Assert(_followComponent != null);
            descriptor.CameraFieldOfView = _camera.fieldOfView;
            descriptor.FollowOffset = _followComponent.FollowOffset;
            descriptor.FollowSmoothness = _followComponent.FollowSmoothness;
        }

        private static int CompareDescriptors(CameraBlendDescriptor left,  CameraBlendDescriptor right)
        {
            int comparison = left.Priority.CompareTo(right.Priority);
            return comparison != 0 ? comparison : left.GetHashCode().CompareTo(right.GetHashCode());
        }
    }
}