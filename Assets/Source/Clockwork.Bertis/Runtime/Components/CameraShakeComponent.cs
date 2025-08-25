using UnityEngine;
using Clockwork.Collections;

#nullable enable

namespace Clockwork.Bertis.Components
{
    public sealed class CameraShakeComponent : MonoBehaviour
    {
        [SerializeField]
        private int _maxSimultaneousShakes;

        [SerializeField]
        private Transform? _targetTransform;

        private readonly Vector<ShakeWorker> _busyWorkers = new();
        private readonly Vector<ShakeWorker> _freeWorkers = new();

        public int MaxSimultaneousShakes => _maxSimultaneousShakes;

        public int ActiveShakes => _busyWorkers.Count;

        public void StartShake(CameraShakeDescriptor? descriptor, Vector3 position)
        {
            if (descriptor == null || !descriptor.IsValid || _targetTransform == null)
            {
                return;
            }

            float scale = !descriptor.IsSpatial ? 1f : descriptor.ResolveScale(transform.position, position);
            if (scale > 0f)
            {
                StartShakeImpl(descriptor, scale);
            }
        }

        public void StartShake(CameraShakeDescriptor? descriptor)
        {
            if (descriptor == null || !descriptor.IsValid || _targetTransform == null)
            {
                return;
            }
            StartShakeImpl(descriptor, scale: 1f);
        }

        private void StartShakeImpl(CameraShakeDescriptor descriptor, float scale)
        {
            if (_busyWorkers.Count >= _maxSimultaneousShakes)
            {
                Debug.LogInformation("StartShake failed: max simultaneous shakes reached.", context: this);
                return;
            }

            if (!_freeWorkers.TryPop(out ShakeWorker? worker))
            {
                worker = new ShakeWorker();
            }
            _busyWorkers.Push(worker);
            worker.Prepare(descriptor, scale);
        }

        private void Update()
        {
            if (_busyWorkers.IsEmpty || _targetTransform == null)
            {
                return;
            }

            (Vector3 nextPosition, Vector3 nextRotation) = (Vector3.zero, Vector3.zero);
            (float Scaled, float Unscaled) deltaTime = (Time.deltaTime, Time.unscaledDeltaTime);

            for (int i = _busyWorkers.Count; --i >= 0;)
            {
                ShakeWorker worker = _busyWorkers[i];
                if (worker.Update(deltaTime, out var evaluation))
                {
                    nextPosition += evaluation.Position;
                    nextRotation += evaluation.Rotation;
                }
                else
                {
                    worker.Reset();
                    _busyWorkers.SwapRemoveAt(i);
                    _freeWorkers.Push(worker);
                }
            }

            if (!_busyWorkers.IsEmpty)
            {
                _targetTransform.SetLocalPositionAndRotation(
                    nextPosition, Math.FromEuler(nextRotation));
            }
            else
            {
                _targetTransform.ResetLocalPositionAndRotation();
            }
        }

        private class ShakeWorker
        {
            private CameraShakeDescriptor? _descriptor;
            private float _scale;
            private float _progress;

            public void Prepare(CameraShakeDescriptor descriptor, float scale)
            {
                _descriptor = descriptor;
                _scale = scale;
                _progress = 0f;
            }

            public void Reset()
            {
                _descriptor = null;
            }

            public bool Update((float Scaled, float Unscaled) deltaTime, out (Vector3 Position, Vector3 Rotation) evaluation)
            {
                Debug.Assert(_descriptor != null);
                _progress += _descriptor.ResolveDeltaTime(deltaTime) / _descriptor.Duration;

                if (_progress >= 1f)
                {
                    evaluation = (default, default);
                    return false;
                }

                evaluation = _descriptor.Evaluate(_progress);
                if (_scale != 0f)
                {
                    evaluation.Position *= _scale;
                    evaluation.Rotation *= _scale;
                }
                return true;
            }
        }
    }
}