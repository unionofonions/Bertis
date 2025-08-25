using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Clockwork.Collections;

#nullable enable

namespace Clockwork.Pooling;

public interface IPoolWorker
{
    void ScheduleEarlyReturn();
}

public static class PrefabRegistry
{
    private static readonly HashMap<Component, PrefabPool> s_prefabToPool;
    private static readonly HashMap<Component, PrefabPool> s_workerToPool;

#if UNITY_EDITOR
    private static readonly HashMap<Type, Transform> s_prefabTypeToParent = new();
#endif

    static PrefabRegistry()
    {
        s_prefabToPool = new HashMap<Component, PrefabPool>(PrefabComparer.Shared);
        s_workerToPool = new HashMap<Component, PrefabPool>(PrefabComparer.Shared);
#if UNITY_EDITOR
        RegisterClearOnUnload();
#endif
    }

    public static bool Rent<T>(
        [NotNullWhen(true)] T? prefab,
        [NotNullWhen(true), NotNullIfNotNull(nameof(prefab))] out T? worker)
        where T : Component
    {
        if (prefab == null)
        {
            worker = null;
            return false;
        }

        if (!s_prefabToPool.TryGetValue(prefab, out PrefabPool? pool))
        {
            pool = new PrefabPool(prefab);
            s_prefabToPool.Add(prefab, pool);
#if UNITY_EDITOR
            Adopt(pool);
#endif
        }

        Component temp = pool.Rent();
        s_workerToPool.Add(temp, pool);
        worker = (T)temp;
        return true;
    }

    public static void Return(Component? worker)
    {
        if (worker == null)
        {
            ReturnNull(worker);
            return;
        }

        if (!s_workerToPool.Remove(worker, out PrefabPool? pool))
        {
            Debug.LogWarning("Unknown worker returned to registry.", context: worker);
            return;
        }

        pool.Return(worker);
    }

    private static void ReturnNull(Component? worker)
    {
        if (worker is null)
        {
            Debug.LogWarning("Null worker returned to registry.");
        }
        else if (s_workerToPool.Remove(worker))
        {
            Debug.LogError("Rented worker returned to registry as destroyed.");
        }
        else
        {
            Debug.LogWarning("Unknown worker returned to registry as destroyed.");
        }
    }

    public static bool PrefabOf<T>(
        [NotNullWhen(true)] T? worker,
        [NotNullWhen(true)] out T? prefab) where T : Component
    {
        if (worker != null && s_workerToPool.TryGetValue(worker, out PrefabPool? pool))
        {
            prefab = (T)pool.Prefab;
            return true;
        }
        prefab = null;
        return false;
    }

#if UNITY_EDITOR
    [Conditional("UNITY_EDITOR")]
    private static void Adopt(PrefabPool pool)
    {
        Type prefabType = pool.Prefab.GetType();

        if (!s_prefabTypeToParent.TryGetValue(prefabType, out Transform? parent))
        {
            parent = ActorHelpers.PersistentTransform($"[PrefabFamily:{prefabType.Name}]");
            s_prefabTypeToParent.Add(prefabType, parent);
        }

        pool.Parent.SetParent(parent, worldPositionStays: false);
    }

    [Conditional("UNITY_EDITOR")]
    private static void RegisterClearOnUnload()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state is PlayModeStateChange.EnteredEditMode)
            {
                s_prefabToPool.Clear();
                s_workerToPool.Clear();
                s_prefabTypeToParent.Clear();
            }
        };
    }
#endif

    private class PrefabPool
    {
        public readonly Component Prefab;
        public readonly Transform Parent;

        private readonly Vector<Component> _freeWorkers;
        private readonly BusyWorkers? _busyWorkers;

        public PrefabPool(Component prefab)
        {
            Prefab = prefab;
            Parent = ActorHelpers.PersistentTransform($"[PrefabPool:{prefab.name}]");
            _freeWorkers = new Vector<Component>(4);

            if (prefab is IPoolWorker && prefab.TryGetComponent(out PrefabConfigProvider configProvider))
            {
                _busyWorkers = new BusyWorkers(configProvider);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Component Rent()
        {
            if (!_freeWorkers.TryPop(out Component? worker))
            {
                worker = Create();
            }
            _busyWorkers?.OnRented(worker);
            return worker;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(Component worker)
        {
            _busyWorkers?.OnReturned(worker);
            _freeWorkers.Push(worker);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Component Create()
            => UnityEngine.Object.Instantiate(Prefab, Parent);

        private class BusyWorkers
        {
            private readonly Deque<Component> _deque;
            private readonly HashSet<Component> _set;
            private readonly PrefabConfigProvider _configProvider;

            public BusyWorkers(PrefabConfigProvider configProvider)
            {
                _deque = new Deque<Component>();
                _set = new HashSet<Component>(PrefabComparer.Shared);
                _configProvider = configProvider;
            }

            public void OnRented(Component worker)
            {
                _deque.PushBack(worker);
                bool added = _set.Add(worker);
                Debug.Assert(added);

                if (_deque.Count > _configProvider.MaxSize)
                {
                    Component front = _deque.PopFront();
                    bool removed = _set.Remove(front);
                    Debug.Assert(removed);
                    ((IPoolWorker)front).ScheduleEarlyReturn();
                }
            }

            public void OnReturned(Component worker)
            {
                if (_set.Remove(worker))
                {
                    if (_deque.PeekFront() == worker)
                    {
                        _ = _deque.PopFront();
                    }
                    else
                    {
                        bool removed = _deque.Remove(worker);
                        Debug.Assert(removed);
                        Debug.LogWarning($"Non-deterministic return behavior found.", context: worker);
                    }
                }
            }

            private void PushBack(Component worker)
            {
                _deque.PushBack(worker);
                bool added = _set.Add(worker);
                Debug.Assert(added);
            }

            private Component PopFront()
            {
                Component worker = _deque.PopFront();
                bool removed = _set.Remove(worker);
                Debug.Assert(removed);
                return worker;
            }
        }
    }

    private class PrefabComparer : IEqualityComparer<Component>
    {
        public static readonly PrefabComparer Shared = new();

        public bool Equals(Component x, Component y) => x == y;

        public int GetHashCode(Component obj) => obj.GetHashCode();
    }
}