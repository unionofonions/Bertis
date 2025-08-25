using System;
using System.Diagnostics.CodeAnalysis;

namespace Clockwork;

public static partial class ThrowHelpers
{
    [DoesNotReturn]
    internal static void ThrowIndexOutOfRange()
        => throw new ArgumentOutOfRangeException("index");

    [DoesNotReturn]
    internal static void ThrowEmptyCollection()
        => throw new InvalidOperationException("Cannot operate on empty collection");

    [DoesNotReturn]
    internal static void ThrowInvalidEnumeration()
        => throw new InvalidOperationException("Collection cannot be modified during enumeration.");

    [DoesNotReturn]
    internal static void ThrowConcurrentOperation()
        => throw new InvalidOperationException("Concurrent operation is unsupported.");

    [DoesNotReturn]
    internal static void ThrowDuplicateKey<TKey>(TKey key)
        => throw new ArgumentException($"Attempted to add duplicate key '{key}'.");

    [DoesNotReturn]
    internal static void ThrowKeyNotFound<TKey>(TKey key)
        => throw new ArgumentException($"Key '{key}' not found.");
}