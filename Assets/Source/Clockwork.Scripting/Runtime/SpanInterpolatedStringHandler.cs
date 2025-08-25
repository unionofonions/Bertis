using System;
using System.Runtime.CompilerServices;

namespace Clockwork.Scripting;

internal static class StringInterpolationHelper
{
    public static ReadOnlySpan<char> F(SpanInterpolatedStringHandler interpolation)
        => interpolation.AsSpan();
}

[InterpolatedStringHandler]
internal ref struct SpanInterpolatedStringHandler
{
    private static readonly char[] s_buffer = new char[256];

    private readonly Span<char> _chars;
    private int _pos;

    public SpanInterpolatedStringHandler(int literalLength, int formattedCount, out bool shouldAppend)
    {
        _chars = s_buffer;
        _pos = 0;
        shouldAppend = true;
    }

    public void AppendLiteral(string value)
    {
        value.AsSpan().CopyTo(_chars.Slice(_pos));
        _pos += value.Length;
    }

    public void AppendFormatted(float value)
    {
        if (value.TryFormat(_chars.Slice(_pos), out int written, default, null))
        {
            _pos += written;
        }
    }

    public void AppendFormatted(float value, ReadOnlySpan<char> format)
    {
        if (value.TryFormat(_chars.Slice(_pos), out int written, format, null))
        {
            _pos += written;
        }
    }

    public void AppendFormatted(int value)
    {
        if (value.TryFormat(_chars.Slice(_pos), out int written, default, null))
        {
            _pos += written;
        }
    }

    public void AppendFormatted(int value, ReadOnlySpan<char> format)
    {
        if (value.TryFormat(_chars.Slice(_pos), out int written, format, null))
        {
            _pos += written;
        }
    }

    public void AppendFormatted(object value)
    {
        if (value is not null)
        {
            string s = value.ToString();
            s.AsSpan().CopyTo(_chars.Slice(_pos));
            _pos += s.Length;
        }
    }

    public ReadOnlySpan<char> AsSpan() => _chars.Slice(0, _pos);
}