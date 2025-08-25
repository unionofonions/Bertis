using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#nullable enable

namespace Clockwork.Collections;

public class Vector<T>
{
    private const int DefaultCapacity = 4;

    private T[] _items;
    private int _count;
    private int _version;

    private static readonly T[] s_emptyArray = new T[0];

    public Vector()
    {
        _items = s_emptyArray;
    }

    public Vector(int capacity)
    {
        ThrowHelpers.ThrowIfNegative(capacity);
        _items = capacity == 0 ? s_emptyArray : new T[capacity];
    }

    public Vector(IEnumerable<T> items)
    {
        ThrowHelpers.ThrowIfNull(items);

        if (items is ICollection<T> collection)
        {
            int count = collection.Count;
            if (count == 0)
            {
                _items = s_emptyArray;
            }
            else
            {
                _items = new T[count];
                collection.CopyTo(_items, 0);
                _count = count;
            }
        }
        else
        {
            _items = s_emptyArray;
            using IEnumerator<T> enumerator = items.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Push(enumerator.Current);
            }
        }
    }

    public int Count => _count;

    public int Capacity
    {
        get => _items.Length;
        set
        {
            if (value < _count)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            if (value != _items.Length)
            {
                if (value > 0)
                {
                    T[] newItems = new T[value];
                    if (_count > 0)
                    {
                        Array.Copy(_items, newItems, _count);
                    }
                    _items = newItems;
                }
                else
                {
                    _items = s_emptyArray;
                }
            }
        }
    }

    public bool IsEmpty => _count == 0;

    public T this[int index]
    {
        get
        {
            if ((uint)index >= (uint)_count)
            {
                ThrowHelpers.ThrowIndexOutOfRange();
            }
            return _items[index];
        }
        set
        {
            if ((uint)index >= (uint)_count)
            {
                ThrowHelpers.ThrowIndexOutOfRange();
            }
            _items[index] = value;
            _version++;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(T item)
    {
        _version++;
        T[] items = _items;
        int count = _count;
        if ((uint)count < (uint)items.Length)
        {
            _count = count + 1;
            items[count] = item;
        }
        else
        {
            PushRare(item);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void PushRare(T item)
    {
        Debug.Assert(_count == _items.Length);
        int count = _count;
        Grow(count + 1);
        _count = count + 1;
        _items[count] = item;
    }

    public void PushRange(IEnumerable<T> items)
    {
        ThrowHelpers.ThrowIfNull(items);

        if (items is ICollection<T> collection)
        {
            int count = collection.Count;
            if (count > 0)
            {
                if (_items.Length - _count < count)
                {
                    Grow(checked(_count + count));
                }
                collection.CopyTo(_items, _count);
                _count += count;
                _version++;
            }
        }
        else
        {
            using IEnumerator<T> enumerator = items.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Push(enumerator.Current);
            }
        }
    }

    public void Insert(int index, T item)
    {
        if ((uint)index > (uint)_count)
        {
            ThrowHelpers.ThrowIndexOutOfRange();
        }

        if (_count == _items.Length)
        {
            GrowForInsertion(index, 1);
        }
        else if (index < _count)
        {
            Array.Copy(_items, index, _items, index + 1, _count - index);
        }

        _items[index] = item;
        _count++;
        _version++;
    }

    public void InsertRange(int index, IEnumerable<T> items)
    {
        ThrowHelpers.ThrowIfNull(items);
        if ((uint)index > (uint)_count)
        {
            ThrowHelpers.ThrowIndexOutOfRange();
        }

        if (items is ICollection<T> collection)
        {
            int count = collection.Count;
            if (count > 0)
            {
                if (_items.Length - _count < count)
                {
                    GrowForInsertion(index, count);
                }
                else if (index < _count)
                {
                    Array.Copy(_items, index, _items, index + count, _count - index);
                }

                if (this == collection)
                {
                    Array.Copy(_items, 0, _items, index, index);
                    Array.Copy(_items, index + count, _items, index * 2, _count - index);
                }
                else
                {
                    collection.CopyTo(_items, index);
                }

                _count += count;
                _version++;
            }
        }
        else
        {
            using IEnumerator<T> enumerator = items.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Insert(index++, enumerator.Current);
            }
        }
    }

    public T Peek()
    {
        T[] items = _items;
        int count = _count - 1;

        if ((uint)count >= (uint)items.Length)
        {
            ThrowHelpers.ThrowEmptyCollection();
        }

        return items[count];
    }

    public bool TryPeek([MaybeNullWhen(false)] out T item)
    {
        T[] items = _items;
        int count = _count - 1;

        if ((uint)count >= (uint)items.Length)
        {
            item = default!;
            return false;
        }

        item = items[count];
        return true;
    }

    public T Pop()
    {
        T[] items = _items;
        int count = _count - 1;

        if ((uint)count >= (uint)items.Length)
        {
            ThrowHelpers.ThrowEmptyCollection();
        }

        _version++;
        _count = count;
        T item = items[count];
        items[count] = default!;
        return item;
    }

    public bool TryPop([MaybeNullWhen(false)] out T item)
    {
        T[] items = _items;
        int count = _count - 1;

        if ((uint)count >= (uint)items.Length)
        {
            item = default;
            return false;
        }

        _version++;
        _count = count;
        item = items[count];
        items[count] = default!;
        return true;
    }

    public void RemoveAt(int index)
    {
        if ((uint)index >= (uint)_count)
        {
            ThrowHelpers.ThrowIndexOutOfRange();
        }

        _count--;
        if (index < _count)
        {
            Array.Copy(_items, index + 1, _items, index, _count - index);
        }
        _items[_count] = default!;
        _version++;
    }

    public bool Remove(T item)
    {
        int index = IndexOf(item);
        if (index >= 0)
        {
            RemoveAt(index);
            return true;
        }
        return false;
    }

    public void RemoveRange(int index, int count)
    {
        ThrowHelpers.ThrowIfNegative(index);
        ThrowHelpers.ThrowIfNegative(count);
        if (_count - index < count)
        {
            throw new ArgumentOutOfRangeException(null, "Invalid range.");
        }

        if (count > 0)
        {
            _count -= count;
            if (index < _count)
            {
                Array.Copy(_items, index + count, _items, index, _count - index);
            }

            _version++;
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                Array.Clear(_items, _count, count);
            }
        }
    }

    public bool SwapRemove(T item)
    {
        int index = IndexOf(item);
        if (index >= 0)
        {
            SwapRemoveAt(index);
            return true;
        }
        return false;
    }

    public void SwapRemoveAt(int index)
    {
        if ((uint)index >= (uint)_count)
        {
            ThrowHelpers.ThrowIndexOutOfRange();
        }

        T[] items = _items;
        int count = --_count;
        items[index] = items[count];
        items[count] = default!;
        _version++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        _version++;
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            int count = _count;
            _count = 0;
            if (count > 0)
            {
                Array.Clear(_items, 0, count);
            }
        }
        else
        {
            _count = 0;
        }
    }

    public void TrimExcess()
    {
        int threshold = (int)(_items.Length * 0.9);
        if (_count < threshold)
        {
            Capacity = _count;
        }
    }

    public bool Contains(T item)
        => _count != 0 && IndexOf(item) >= 0;

    public int IndexOf(T item)
        => Array.IndexOf(_items, item, 0, _count);

    public int EnsureCapacity(int capacity)
    {
        ThrowHelpers.ThrowIfNegative(capacity);
        if (_items.Length < capacity)
        {
            Grow(capacity);
        }
        return _items.Length;
    }

    public Enumerator GetEnumerator()
        => new Enumerator(this);

    private void Grow(int capacity)
        => Capacity = GetNewCapacity(capacity);

    private void GrowForInsertion(int insertIndex, int insertionCount)
    {
        Debug.Assert(insertionCount > 0);

        int requiredCapacity = checked(_count + insertionCount);
        int newCapacity = GetNewCapacity(requiredCapacity);
        T[] newItems = new T[newCapacity];

        if (insertIndex != 0)
        {
            Array.Copy(_items, newItems, insertIndex);
        }
        if (_count != insertIndex)
        {
            Array.Copy(_items, insertIndex, newItems, insertIndex + insertionCount, _count - insertIndex);
        }

        _items = newItems;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetNewCapacity(int capacity)
    {
        Debug.Assert(_items.Length < capacity);
        int newCapacity = _items.Length == 0 ? DefaultCapacity : 2 * _items.Length;
        return Math.Max(newCapacity, capacity);
    }

    public struct Enumerator : IEnumerator<T>, IEnumerator
    {
        private readonly Vector<T> _vector;
        private int _index;
        private T? _current;
        private readonly int _version;

        internal Enumerator(Vector<T> vector)
        {
            _vector = vector;
            _index = 0;
            _current = default;
            _version = vector._version;
        }

        public readonly T Current => _current!;

        readonly object? IEnumerator.Current
        {
            get
            {
                if (_index == 0 || _index == _vector._count + 1)
                {
                    ThrowHelpers.ThrowInvalidEnumeration();
                }
                return Current;
            }
        }

        public bool MoveNext()
        {
            Vector<T> vector = _vector;

            if (_version == vector._version && ((uint)_index < (uint)vector._count))
            {
                _current = vector._items[_index];
                _index++;
                return true;
            }
            return MoveNextRare();
        }

        private bool MoveNextRare()
        {
            if (_version != _vector._version)
            {
                ThrowHelpers.ThrowInvalidEnumeration();
            }
            _index = _vector._count + 1;
            _current = default;
            return false;
        }

        void IEnumerator.Reset()
        {
            if (_version != _vector._version)
            {
                ThrowHelpers.ThrowInvalidEnumeration();
            }
            _index = 0;
            _current = default;
        }

        public readonly void Dispose()
        {
        }
    }
}