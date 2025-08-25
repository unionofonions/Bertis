using System;
using UnityEngine;
using Clockwork.Collections;

#nullable enable

namespace Clockwork.Simulation
{
    public static class TimeDilationSystem
    {
        private const float TimeScaleMin = 0.2f;

        private const float TimeScaleMax = 1.5f;

        public const int MaxSimultaneousDilations = 4;

        private static readonly float DefaultFixedDeltaTime = Time.fixedDeltaTime;

        private static DilationUpdater? s_updater;

        public static event Action<float>? TimeScaleChanged;

        public static int ActiveDilations
        {
            get
            {
                if (s_updater == null)
                {
                    return 0;
                }
                return s_updater.ActiveDilations;
            }
        }

        private static DilationUpdater Updater
        {
            get
            {
                if (s_updater == null)
                {
                    s_updater = ActorHelpers.PersistentComponent<DilationUpdater>("[TimeDilationUpdater]");
                }
                return s_updater;
            }
        }

        public static void StartTimeDilation(TimeDilationDescriptor? descriptor)
        {
            if (descriptor == null || !descriptor.IsValid)
            {
                return;
            }
            if (Updater.ActiveDilations >= MaxSimultaneousDilations)
            {
                Debug.LogInformation("StartTimeDilation failed: max simultaneous dilations reached.", context: descriptor);
                return;
            }

            Updater.StartTimeDilation(descriptor);
        }

        private static void SetTimeScale(float value)
        {
            value = Math.Clamp(value, TimeScaleMin, TimeScaleMax);
            Time.timeScale = value;
            Time.fixedDeltaTime = value * DefaultFixedDeltaTime;
            TimeScaleChanged?.Invoke(value);
        }

#if UNITY_EDITOR
        [Unity.Profiling.Editor.ProfilerModuleMetadata("TimeDilationUpdater")]
#endif
        private class DilationUpdater : MonoBehaviour
        {
            private readonly Vector<DilationWorker> _busyWorkers = new();
            private readonly Vector<DilationWorker> _freeWorkers = new();

            public int ActiveDilations => _busyWorkers.Count;

            public void StartTimeDilation(TimeDilationDescriptor descriptor)
            {
                if (!_freeWorkers.TryPop(out DilationWorker? worker))
                {
                    worker = new DilationWorker();
                }

                _busyWorkers.Push(worker);
                worker.Prepare(descriptor);

                if (_busyWorkers.Count == 1)
                {
                    enabled = true;
                }
            }

            private void Update()
            {
                Debug.Assert(!_busyWorkers.IsEmpty);

                float nextTimeScale = 1f;
                float deltaTime = Time.unscaledDeltaTime;

                for (int i = _busyWorkers.Count; --i >= 0;)
                {
                    DilationWorker worker = _busyWorkers[i];
                    if (worker.Update(deltaTime, out float timeScale))
                    {
                        nextTimeScale += timeScale;
                    }
                    else
                    {
                        _busyWorkers.SwapRemoveAt(i);
                        _freeWorkers.Push(worker);
                        worker.Reset();
                    }
                }

                if (!_busyWorkers.IsEmpty)
                {
                    SetTimeScale(nextTimeScale);
                }
                else
                {
                    SetTimeScale(1f);
                    enabled = false;
                }
            }
        }

        private class DilationWorker
        {
            private TimeDilationDescriptor? _descriptor;
            private float _progress;

            public void Prepare(TimeDilationDescriptor descriptor)
            {
                _descriptor = descriptor;
                _progress = 0f;
            }

            public void Reset()
            {
                _descriptor = null;
            }

            public bool Update(float deltaTime, out float timeScale)
            {
                Debug.Assert(_descriptor != null);
                _progress += deltaTime / _descriptor.Duration;

                if (_progress >= 1f)
                {
                    timeScale = 0;
                    return false;
                }

                timeScale = _descriptor.Evaluate(_progress);
                return true;
            }
        }
    }
}