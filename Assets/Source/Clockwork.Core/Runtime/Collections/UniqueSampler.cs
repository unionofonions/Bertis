using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable

namespace Clockwork.Collections;

[Serializable]
public class UniqueSampler<T>
{
    [SerializeField]
    private T?[] _items;

    [NonSerialized]
    private int[]? _indices;

    [SerializeField]
    private int _uniqueSamples;

    [NonSerialized]
    private int _bufferLength;

    [NonSerialized]
    private int _bufferOffset;

    [NonSerialized]
    private readonly bool _keepOrder = !Application.isEditor;

    [NonSerialized]
    private readonly Random? _random;

    public UniqueSampler(IEnumerable<T?> items)
    {
        ThrowHelpers.ThrowIfNull(items);

        if (items is ICollection<T?> collection)
        {
            _items = new T[collection.Count];
            collection.CopyTo(_items, 0);
        }
        else
        {
            _items = items.ToArray();
        }
    }

    public UniqueSampler(T?[] items, bool copyItems)
    {
        ThrowHelpers.ThrowIfNull(items);

        if (copyItems)
        {
            _items = new T?[items.Length];
            items.CopyTo(_items, 0);
        }
        else
        {
            _items = items;
        }
    }

    public required bool KeepOrder
    {
        init => _keepOrder = value;
    }

    public Random? Random
    {
        init => _random = value;
    }

    public int Count => _items.Length;

    public int UniqueSamples
    {
        set
        {
            _uniqueSamples = Math.Max(0, Math.Min(value, _items.Length - 1));
        }
    }

    public T? this[int index]
    {
        get
        {
            if ((uint)index >= (uint)_items.Length)
            {
                ThrowHelpers.ThrowIndexOutOfRange();
            }
            return _items[index];
        }
    }

    public T? Sample()
    {
        return _items.Length switch
        {
            0 => default,
            1 => _items[0],
            _ when _keepOrder => SampleOrdered(),
            _ => SampleUnordered()
        };
    }

    public void Reset()
    {
        _bufferLength = 0;
        _bufferOffset = 0;
    }

    private T? SampleOrdered()
    {
        int[]? indices = _indices;
        Random random = _random ?? Random.Shared;

        if (indices?.Length != _items.Length)
        {
            _bufferLength = 0;
            _bufferOffset = 0;
            _indices = indices = CreateIndices(_items.Length);
        }

        int index = random.NextInt32(_bufferLength, indices.Length);
        int result = indices[index];

        indices[index] = indices[_bufferOffset];
        indices[_bufferOffset] = result;

        _bufferLength = Math.Min(_uniqueSamples, _bufferLength + 1);
        _bufferOffset = _bufferOffset + 1 >= _uniqueSamples ? 0 : _bufferOffset + 1;

        return _items[result];
    }

    private T? SampleUnordered()
    {
        T?[] items = _items;
        Random random = _random ?? Random.Shared;

        int index = random.NextInt32(_bufferLength, items.Length);
        T? result = items[index];

        items[index] = items[_bufferOffset];
        items[_bufferOffset] = result;

        _bufferLength = Math.Min(_uniqueSamples, _bufferLength + 1);
        _bufferOffset = _bufferOffset + 1 >= _uniqueSamples ? 0 : _bufferOffset + 1;

        return result;
    }

    private static int[] CreateIndices(int length)
    {
        int[] result = new int[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = i;
        }
        return result;
    }
}