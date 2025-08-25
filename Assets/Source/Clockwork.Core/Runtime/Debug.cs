using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace Clockwork;

public enum LogLevel
{
    Trace = 0,
    Information = 1,
    Warning = 2,
    Error = 3
}

public delegate void LogAction(LogLevel level, object? message, object? context);

public static class Debug
{
    public static event LogAction? Logged;

    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void LogTrace(object? message, object? context = null)
        => OnLogged(LogLevel.Trace, message, context);

    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void LogInformation(object? message, object? context = null)
        => OnLogged(LogLevel.Information, message, context);

    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void LogWarning(object? message, object? context = null)
        => OnLogged(LogLevel.Warning, message, context);

    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void LogError(object? message, object? context = null)
        => OnLogged(LogLevel.Error, message, context);

    private static void OnLogged(LogLevel level, object? message, object? context)
        => Logged?.Invoke(level, message, context);

    [Conditional("DEBUG")]
    public static void Assert([DoesNotReturnIf(false)] bool condition, string? message = null)
    {
        if (!condition)
        {
            Fail(message);
        }
    }

    [DoesNotReturn]
    public static void Fail(string? message)
        => throw new AssertionException(message);

    private sealed class AssertionException : Exception
    {
        public AssertionException(string? message) : base(message) { }
    }
}