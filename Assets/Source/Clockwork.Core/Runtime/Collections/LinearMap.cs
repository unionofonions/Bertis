using System;
using System.Collections.Generic;
using UnityEngine;

namespace Clockwork.Collections
{
    [Serializable]
    public class LinearMap<TKey, TValue>
    {
        [SerializeField]
        private Entry[] _entries;

        public bool TryGetValue(TKey key, out TValue value)
        {
            for (int i = 0; i < _entries.Length; i++)
            {
                if (EqualityComparer<TKey>.Default.Equals(_entries[i].Key, key))
                {
                    value = _entries[i].Value;
                    return true;
                }
            }
            value = default;
            return false;
        }

        [Serializable]
        private struct Entry
        {
            public TKey Key;
            public TValue Value;
        }
    }
}