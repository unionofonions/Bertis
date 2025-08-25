using System.Runtime.CompilerServices;

#nullable enable

namespace Clockwork.Collections;

internal static class HashHelpers
{
    public const int MaxPrimeArrayLength = 0x7FFFFFC3;

    public const int HashPrime = 101;

    private static readonly int[] Primes =
    {
        3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163,
        197, 239, 293, 353, 431, 512, 631, 761, 919, 1103, 1327,
        1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419
    };

    public static bool IsPrime(int value)
    {
        if ((value & 1) != 0)
        {
            int limit = (int)Math.Sqrt(value);
            for (int divisor = 3; divisor <= limit; divisor += 2)
            {
                if ((value % divisor) == 0)
                {
                    return false;
                }
            }
            return true;
        }
        return value == 2;
    }

    public static int GetPrime(int minValue)
    {
        ThrowHelpers.ThrowIfNegative(minValue);
        foreach (int prime in Primes)
        {
            if (prime >= minValue)
            {
                return prime;
            }
        }
        for (int i = minValue | 1; i < int.MaxValue; i += 2)
        {
            if (IsPrime(i) && ((i - 1) % HashPrime != 0))
            {
                return i;
            }
        }
        return minValue;
    }

    public static int ExpandPrime(int oldSize)
    {
        int newSize = 2 * oldSize;
        if ((uint)newSize > MaxPrimeArrayLength && MaxPrimeArrayLength > oldSize)
        {
            Debug.Assert(MaxPrimeArrayLength == GetPrime(MaxPrimeArrayLength));
            return MaxPrimeArrayLength;
        }
        return GetPrime(newSize);
    }

    public static ulong GetFastModMultiplier(uint divisor)
        => ulong.MaxValue / divisor + 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint FastMod(uint value, uint divisor, ulong multiplier)
    {
        Debug.Assert(divisor <= int.MaxValue);
        uint high = (uint)(((((multiplier * value) >> 32) + 1) * divisor) >> 32);
        Debug.Assert(high == value % divisor);
        return high;
    }
}