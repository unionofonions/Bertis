using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#nullable enable

namespace Clockwork.Collections;

public class HashMap<TKey, TValue>
{
    private Entry[]? _entries;
    private int[]? _buckets;
#if UNITY_64
    private ulong _fastModMultiplier;
#endif
    private int _count;
    private int _freeList;
    private int _freeCount;
    private int _version;
    private readonly IEqualityComparer<TKey>? _comparer;
    private const int StartOfFreeList = -3;

    public HashMap()
        : this(capacity: 0, comparer: null)
    {
    }

    public HashMap(int capacity)
        : this(capacity, comparer: null)
    {
    }

    public HashMap(IEqualityComparer<TKey>? comparer)
        : this(capacity: 0, comparer)
    {
    }

    public HashMap(int capacity, IEqualityComparer<TKey>? comparer)
    {
        ThrowHelpers.ThrowIfNegative(capacity);

        if (capacity > 0)
        {
            Initialize(capacity);
        }
        if (default(TKey) == null)
        {
            _comparer = comparer ?? EqualityComparer<TKey>.Default;
        }
        else if (comparer is not null && comparer != EqualityComparer<TKey>.Default)
        {
            _comparer = comparer;
        }
    }

    public int Count => _count - _freeCount;

    public int Capacity => _entries?.Length ?? 0;

    public IEqualityComparer<TKey> Comparer => _comparer ?? EqualityComparer<TKey>.Default;

    public TValue this[TKey key]
    {
        get
        {
            ref TValue value = ref FindValue(key);
            if (!Unsafe.IsNullRef(ref value))
            {
                return value;
            }
            else
            {
                ThrowHelpers.ThrowKeyNotFound(key);
                return default;
            }
        }
        set
        {
            bool modified = TryInsert(key, value, InsertionBehavior.OverwriteExisting);
            Debug.Assert(modified);
        }
    }

    public void Add(TKey key, TValue value)
    {
        bool modified = TryInsert(key, value, InsertionBehavior.ThrowOnExisting);
        Debug.Assert(modified);
    }

    public bool TryAdd(TKey key, TValue value)
        => TryInsert(key, value, InsertionBehavior.None);

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        ref TValue valueRef = ref FindValue(key);
        if (!Unsafe.IsNullRef(ref valueRef))
        {
            value = valueRef;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    public bool ContainsKey(TKey key)
        => !Unsafe.IsNullRef(ref FindValue(key));

    public bool Remove(TKey key)
    {
        ThrowHelpers.ThrowIfNull(key);

        if (_buckets is not null)
        {
            Entry[]? entries = _entries;
            Debug.Assert(entries is not null);

            IEqualityComparer<TKey>? comparer = _comparer;
            Debug.Assert(default(TKey) != null || comparer is not null);

            uint hashCode = (uint)(default(TKey) != null && comparer is null ? key.GetHashCode() : comparer!.GetHashCode(key));
            ref int bucket = ref GetBucket(hashCode);
            int last = -1;
            uint collisionCount = 0;

            for (int i = bucket - 1; i >= 0;)
            {
                ref Entry entry = ref entries[i];

                if (entry.HashCode == hashCode && (default(TKey) != null && comparer is null
                    ? EqualityComparer<TKey>.Default.Equals(entry.Key, key) : comparer!.Equals(entry.Key, key)))
                {
                    if (last < 0)
                    {
                        bucket = entry.Next + 1;
                    }
                    else
                    {
                        entries[last].Next = entry.Next;
                    }

                    Debug.Assert((StartOfFreeList - _freeList) < 0);
                    entry.Next = StartOfFreeList - _freeList;
                    entry.Key = default!;
                    entry.Value = default!;

                    _freeList = i;
                    _freeCount++;
                    return true;
                }

                last = i;
                i = entry.Next;
                collisionCount++;

                if (collisionCount > (uint)entries.Length)
                {
                    ThrowHelpers.ThrowConcurrentOperation();
                }
            }
        }

        return false;
    }

    public bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        ThrowHelpers.ThrowIfNull(key);

        if (_buckets is not null)
        {
            Entry[]? entries = _entries;
            Debug.Assert(entries is not null);

            IEqualityComparer<TKey>? comparer = _comparer;
            Debug.Assert(default(TKey) != null || comparer is not null);

            uint hashCode = (uint)(default(TKey) != null && comparer is null ? key.GetHashCode() : comparer!.GetHashCode(key));
            ref int bucket = ref GetBucket(hashCode);
            int last = -1;
            uint collisionCount = 0;

            for (int i = bucket - 1; i >= 0;)
            {
                ref Entry entry = ref entries[i];

                if (entry.HashCode == hashCode && (default(TKey) != null && comparer is null
                    ? EqualityComparer<TKey>.Default.Equals(entry.Key, key) : comparer!.Equals(entry.Key, key)))
                {
                    if (last < 0)
                    {
                        bucket = entry.Next + 1;
                    }
                    else
                    {
                        entries[last].Next = entry.Next;
                    }

                    value = entry.Value;

                    Debug.Assert((StartOfFreeList - _freeList) < 0);
                    entry.Next = StartOfFreeList - _freeList;
                    entry.Key = default!;
                    entry.Value = default!;

                    _freeList = i;
                    _freeCount++;
                    return true;
                }

                last = i;
                i = entry.Next;
                collisionCount++;

                if (collisionCount > (uint)entries.Length)
                {
                    ThrowHelpers.ThrowConcurrentOperation();
                }
            }
        }

        value = default;
        return false;
    }

    public void Clear()
    {
        int count = _count;
        if (count > 0)
        {
            Debug.Assert(_entries is not null);
            Debug.Assert(_buckets is not null);

            Array.Clear(_buckets, 0, _buckets.Length);
            _count = 0;
            _freeList = -1;
            _freeCount = 0;
            Array.Clear(_entries, 0, count);
        }
    }

    private bool TryInsert(TKey key, TValue value, InsertionBehavior behavior)
    {
        ThrowHelpers.ThrowIfNull(key);

        if (_buckets is null)
        {
            Initialize(0);
        }
        Debug.Assert(_buckets is not null);

        Entry[]? entries = _entries;
        Debug.Assert(entries is not null);

        IEqualityComparer<TKey>? comparer = _comparer;
        Debug.Assert(comparer is not null || default(TKey) != null);

        uint hashCode = (uint)((default(TKey) != null && comparer is null) ? key.GetHashCode() : comparer!.GetHashCode(key));
        ref int bucket = ref GetBucket(hashCode);
        int i = bucket - 1;
        uint collisionCount = 0;

        if (default(TKey) != null && comparer is null)
        {
            while ((uint)i < (uint)entries.Length)
            {
                if (entries[i].HashCode == hashCode && EqualityComparer<TKey>.Default.Equals(entries[i].Key, key))
                {
                    if (behavior is InsertionBehavior.OverwriteExisting)
                    {
                        entries[i].Value = value;
                        return true;
                    }
                    if (behavior is InsertionBehavior.ThrowOnExisting)
                    {
                        ThrowHelpers.ThrowDuplicateKey(key);
                    }
                    return false;
                }

                i = entries[i].Next;
                collisionCount++;

                if (collisionCount > (uint)entries.Length)
                {
                    ThrowHelpers.ThrowConcurrentOperation();
                }
            }
        }
        else
        {
            Debug.Assert(comparer is not null);
            while ((uint)i < (uint)entries.Length)
            {
                if (entries[i].HashCode == hashCode && comparer.Equals(entries[i].Key, key))
                {
                    if (behavior is InsertionBehavior.OverwriteExisting)
                    {
                        entries[i].Value = value;
                        return true;
                    }
                    if (behavior is InsertionBehavior.ThrowOnExisting)
                    {
                        ThrowHelpers.ThrowDuplicateKey(key);
                    }
                    return false;
                }

                i = entries[i].Next;
                collisionCount++;

                if (collisionCount > (uint)entries.Length)
                {
                    ThrowHelpers.ThrowConcurrentOperation();
                }
            }
        }

        int index;
        if (_freeCount > 0)
        {
            index = _freeList;
            Debug.Assert((StartOfFreeList - entries[_freeList].Next) >= -1);
            _freeList = StartOfFreeList - entries[_freeList].Next;
            _freeCount--;
        }
        else
        {
            int count = _count;
            if (count == entries.Length)
            {
                Resize();
                bucket = ref GetBucket(hashCode);
            }
            index = count;
            _count = count + 1;
            entries = _entries;
        }

        ref Entry entry = ref entries![index];
        entry.HashCode = hashCode;
        entry.Next = bucket - 1;
        entry.Key = key;
        entry.Value = value;
        bucket = index + 1;
        _version++;

        return true;
    }

    private ref TValue FindValue(TKey key)
    {
        ThrowHelpers.ThrowIfNull(key);

        ref Entry entry = ref Unsafe.NullRef<Entry>();
        if (_buckets is not null)
        {
            Debug.Assert(_entries is not null);
            IEqualityComparer<TKey>? comparer = _comparer;

            if (default(TKey) != null && comparer is null)
            {
                Entry[] entries = _entries;
                uint hashCode = (uint)EqualityComparer<TKey>.Default.GetHashCode(key);
                int i = GetBucket(hashCode);
                uint collisionCount = 0;

                i--;
                do
                {
                    if ((uint)i >= (uint)entries.Length)
                    {
                        goto ReturnNotFound;
                    }

                    entry = ref entries[i];
                    if (entry.HashCode == hashCode && EqualityComparer<TKey>.Default.Equals(entry.Key, key))
                    {
                        goto ReturnFound;
                    }

                    i = entry.Next;
                    collisionCount++;
                }
                while (collisionCount <= (uint)entries.Length);

                goto ConcurrentOperation;
            }
            else
            {
                Debug.Assert(comparer is not null);
                Entry[] entries = _entries;
                uint hashCode = (uint)comparer.GetHashCode(key);
                int i = GetBucket(hashCode);
                uint collisionCount = 0;

                i--;
                do
                {
                    if ((uint)i >= (uint)entries.Length)
                    {
                        goto ReturnNotFound;
                    }

                    entry = ref entries[i];
                    if (entry.HashCode == hashCode && comparer.Equals(entry.Key, key))
                    {
                        goto ReturnFound;
                    }

                    i = entry.Next;
                    collisionCount++;
                }
                while (collisionCount <= (uint)entries.Length);

                goto ConcurrentOperation;
            }
        }

        goto ReturnNotFound;

    ConcurrentOperation:
        ThrowHelpers.ThrowConcurrentOperation();
    ReturnFound:
        ref TValue value = ref entry.Value;
    Return:
        return ref value;
    ReturnNotFound:
        value = ref Unsafe.NullRef<TValue>();
        goto Return;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref int GetBucket(uint hashCode)
    {
        int[] buckets = _buckets!;
#if UNITY_64
        return ref buckets[HashHelpers.FastMod(hashCode, (uint)buckets.Length, _fastModMultiplier)];
#else
        return ref buckets[hashCode % buckets.Length];
#endif
    }

    private int Initialize(int capacity)
    {
        int size = HashHelpers.GetPrime(capacity);
        Entry[] entries = new Entry[size];
        int[] buckets = new int[size];

#if UNITY_64
        _fastModMultiplier = HashHelpers.GetFastModMultiplier((uint)size);
#endif
        _freeList = -1;
        _buckets = buckets;
        _entries = entries;

        return size;
    }

    private void Resize()
        => Resize(HashHelpers.ExpandPrime(_count));

    private void Resize(int newSize)
    {
        Debug.Assert(_entries is not null);
        Debug.Assert(newSize >= _entries.Length);

        Entry[] entries = new Entry[newSize];
        int count = _count;
        Array.Copy(_entries, entries, count);

        _buckets = new int[newSize];
#if UNITY_64
        _fastModMultiplier = HashHelpers.GetFastModMultiplier((uint)newSize);
#endif

        for (int i = 0; i < count; i++)
        {
            if (entries[i].Next >= -1)
            {
                ref int bucket = ref GetBucket(entries[i].HashCode);
                entries[i].Next = bucket - 1;
                bucket = i + 1;
            }
        }

        _entries = entries;
    }

    private struct Entry
    {
        public TKey Key;
        public TValue Value;
        public uint HashCode;
        public int Next;
    }
}