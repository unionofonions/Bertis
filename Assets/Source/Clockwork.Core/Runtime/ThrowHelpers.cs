using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#nullable enable

namespace Clockwork;

public static partial class ThrowHelpers
{
    public static void ThrowIfNull(
        [NotNull] object? argument,
        [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument is null)
        {
            ThrowNull(paramName);
        }
    }

    public static void ThrowIfNegative(
        int argument,
        [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument < 0)
        {
            ThrowNegative(paramName);
        }
    }

    [DoesNotReturn]
    public static void ThrowUndefinedEnumIndex<T>(
        T index,
        [CallerArgumentExpression(nameof(index))] string? paramName = null) where T : Enum
        => throw new ArgumentOutOfRangeException(paramName, $"Enum index '{index}' is undefined.");

    [DoesNotReturn]
    private static void ThrowNull(string? paramName)
        => throw new ArgumentNullException(paramName);

    [DoesNotReturn]
    private static void ThrowNegative(string? paramName)
        => throw new ArgumentOutOfRangeException(paramName);
}