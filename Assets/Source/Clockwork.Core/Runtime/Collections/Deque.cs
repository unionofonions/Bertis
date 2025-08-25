using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#nullable enable

namespace Clockwork.Collections;

public class Deque<T>
{
    private const int DefaultCapacity = 4;

    private T[] _items;
    private int _head;
    private int _tail;
    private int _count;

    public Deque()
    {
        _items = new T[DefaultCapacity];
    }

    public Deque(int capacity)
    {
        ThrowHelpers.ThrowIfNegative(capacity);
        _items = new T[capacity == 0 ? DefaultCapacity : Math.NextPow2((uint)capacity)];
    }

    public int Count => _count;

    public int Capacity => _items.Length;

    public T this[int index]
    {
        get
        {
            if ((uint)index >= (uint)_count)
            {
                ThrowHelpers.ThrowIndexOutOfRange();
            }
            return _items[WrapIndex(_head + index)];
        }
        set
        {
            if ((uint)index >= (uint)_count)
            {
                ThrowHelpers.ThrowIndexOutOfRange();
            }
            _items[WrapIndex(_head + index)] = value;
        }
    }

    public void PushFront(T item)
    {
        if (_count == _items.Length)
        {
            Resize();
        }
        _head = WrapIndex(_head - 1);
        _items[_head] = item;
        _count++;
    }

    public void PushBack(T item)
    {
        if (_count == _items.Length)
        {
            Resize();
        }
        _items[_tail] = item;
        _tail = WrapIndex(_tail + 1);
        _count++;
    }

    public T PopFront()
    {
        if (_count == 0)
        {
            ThrowHelpers.ThrowEmptyCollection();
        }

        T item = _items[_head];
        _items[_head] = default!;
        _head = WrapIndex(_head + 1);
        _count--;
        return item;
    }

    public T PopBack()
    {
        if (_count == 0)
        {
            ThrowHelpers.ThrowEmptyCollection();
        }

        _tail = WrapIndex(_tail - 1);
        T item = _items[_tail];
        _items[_tail] = default!;
        _count--;
        return item;
    }

    public T PeekFront()
    {
        if (_count == 0)
        {
            ThrowHelpers.ThrowEmptyCollection();
        }
        return _items[_head];
    }

    public T PeekBack()
    {
        if (_count == 0)
        {
            ThrowHelpers.ThrowEmptyCollection();
        }
        return _items[WrapIndex(_tail - 1)];
    }

    public bool TryPeekFront([MaybeNullWhen(false)] out T item)
    {
        if (_count == 0)
        {
            item = default;
            return false;
        }
        item = _items[_head];
        return true;
    }

    public bool TryPeekBack([MaybeNullWhen(false)] out T item)
    {
        if (_count == 0)
        {
            item = default;
            return false;
        }
        item = _items[WrapIndex(_tail - 1)];
        return true;
    }

    public bool TryPopFront([MaybeNullWhen(false)] out T item)
    {
        if (_count == 0)
        {
            item = default;
            return false;
        }

        item = _items[_head];
        _items[_head] = default!;
        _head = WrapIndex(_head + 1);
        _count--;
        return true;
    }

    public bool TryPopBack([MaybeNullWhen(false)] out T item)
    {
        if (_count == 0)
        {
            item = default;
            return false;
        }

        _tail = WrapIndex(_tail - 1);
        item = _items[_tail];
        _items[_tail] = default!;
        _count--;
        return true;
    }

    public bool Remove(T item)
    {
        for (int i = 0; i < _count; i++)
        {
            T elem = _items[WrapIndex(_head + i)];
            if (EqualityComparer<T>.Default.Equals(elem, item))
            {
                RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    public void RemoveAt(int index)
    {
        if ((uint)index >= (uint)_items.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (index < _count / 2)
        {
            for (int i = index; i > 0; i--)
            {
                int curr = WrapIndex(_head + i);
                int prev = WrapIndex(_head + i - 1);
                _items[curr] = _items[prev];
            }
            _items[_head] = default!;
            _head = WrapIndex(_head + 1);
        }
        else
        {
            for (int i = index; i < _count - 1; i++)
            {
                int curr = WrapIndex(_head + i);
                int next = WrapIndex(_head + i + 1);
                _items[curr] = _items[next];
            }
            _tail = WrapIndex(_tail - 1);
            _items[_tail] = default!;
        }
        _count--;
    }

    public void Clear()
    {
        if (_count > 0)
        {
            Array.Clear(_items, 0, _items.Length);
            _head = 0;
            _tail = 0;
            _count = 0;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int WrapIndex(int index)
        => index & (_items.Length - 1);

    private void Resize()
    {
        int newCapacity = 2 * _items.Length;
        T[] newItems = new T[newCapacity];

        if (_count > 0)
        {
            if (_head < _tail)
            {
                Array.Copy(_items, _head, newItems, 0, _count);
            }
            else
            {
                int front = _items.Length - _head;
                Array.Copy(_items, _head, newItems, 0, front);
                Array.Copy(_items, 0, newItems, front, _tail);
            }
        }

        _items = newItems;
        _head = 0;
        _tail = _count;
    }
}