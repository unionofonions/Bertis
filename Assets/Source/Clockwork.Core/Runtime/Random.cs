using System;
using System.Runtime.CompilerServices;
using UnityEngine;

#nullable enable

namespace Clockwork;

public class Random
{
    private static ulong s_seed = (ulong)DateTime.Now.Ticks;

    private ulong _s0, _s1, _s2, _s3;

    public Random()
        : this(Mix(ref s_seed))
    {
    }

    public Random(ulong seed)
    {
        _s0 = Mix(ref seed);
        _s1 = Mix(ref seed);
        _s2 = Mix(ref seed);
        _s3 = Mix(ref seed);
        Debug.Assert((_s0 | _s1 | _s2 | _s3) != 0uL);
    }

    public static Random Shared { get; } = CreateShared();

    public int NextInt32()
    {
        while (true)
        {
            ulong result = NextUInt64() >> 33;
            if (result != int.MaxValue)
            {
                return (int)result;
            }
        }
    }

    public int NextInt32(int maxValue)
    {
        if (maxValue <= 0)
        {
            Debug.LogWarning("Random.NextInt32.maxValue is <= 0, returning 0.");
            return 0;
        }
        return (int)NextUInt32((uint)maxValue);
    }

    public int NextInt32(int minValue, int maxValue)
    {
        if (minValue > maxValue)
        {
            (minValue, maxValue) = (maxValue, minValue);
        }
        return (int)NextUInt32((uint)(maxValue - minValue)) + minValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float NextSingle()
        => (NextUInt64() >> 40) * (1f / (1u << 24));

    public float NextSingle(float maxValue)
        => NextSingle() * 2f * maxValue - maxValue;

    public float NextSingle(float minValue, float maxValue)
        => NextSingle() * (maxValue - minValue) + minValue;

    public bool NextBoolean(float trueProbability)
        => NextSingle() < trueProbability;

    public Vector2 NextVector2()
        => new(NextSingle(), NextSingle());

    public Vector2 NextVector2(Vector2 maxValue)
        => NextVector2() * 2f * maxValue - maxValue;

    public Vector2 NextVector2(Vector2 minValue, Vector2 maxValue)
        => NextVector2() * (maxValue - minValue) + minValue;

    public Vector3 NextVector3()
        => new(NextSingle(), NextSingle(), NextSingle());

    public Vector3 NextVector3(Vector3 maxValue)
        => Math.Mul(NextVector3() * 2f, maxValue) - maxValue;

    public Vector3 NextVector3(Vector3 minValue, Vector3 maxValue)
        => Math.Mul(NextVector3(), maxValue - minValue) + minValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint NextUInt32()
        => (uint)(NextUInt64() >> 32);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint NextUInt32(uint maxValue)
    {
        ulong product = (ulong)maxValue * NextUInt32();
        uint low = (uint)product;

        if (low < maxValue)
        {
            uint remainder = (0u - maxValue) % maxValue;
            while (low < remainder)
            {
                product = (ulong)maxValue * NextUInt32();
                low = (uint)product;
            }
        }

        return (uint)(product >> 32);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong NextUInt64()
    {
        ulong s0 = _s0, s1 = _s1, s2 = _s2, s3 = _s3;
        ulong result = Math.Rol(s1 * 5uL, 7) * 9uL;
        ulong t = s1 << 17;

        s2 ^= s0;
        s3 ^= s1;
        s1 ^= s2;
        s0 ^= s3;
        s2 ^= t;
        s3 = Math.Rol(s3, 45);

        _s0 = s0;
        _s1 = s1;
        _s2 = s2;
        _s3 = s3;

        return result;
    }

    private static Random CreateShared()
    {
        Debug.LogInformation($"Random.Shared created with seed {s_seed}.");
        return new Random();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Mix(ref ulong x)
    {
        ulong z = x += 0x9E3779B97F4A7C15uL;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9uL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBuL;
        return z ^ (z >> 31);
    }
}