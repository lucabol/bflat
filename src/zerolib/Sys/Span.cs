namespace Sys;

public static partial class SpanUtils
{
    // Workaround for lack of implicit conversions from Span<T> to ReadOnlySpan<T>.
    // ZERO: add the above to Span or make Span/ROSpan partial classes.
    public static System.ReadOnlySpan<T> AsReadOnlySpan<T>(this System.Span<T> span)
        => System.Runtime.InteropServices.MemoryMarshal.CreateReadOnlySpan(ref span[0], span.Length);

    public static System.Span<T> Slice<T>(this System.Span<T> span, int start, int length)
        => System.Runtime.InteropServices.MemoryMarshal.CreateSpan(ref span[start], length);

    public unsafe static System.ReadOnlySpan<T> Slice<T>(this System.ReadOnlySpan<T> span, int start, int length)
    { fixed(void* p = &(span[start])) return new System.ReadOnlySpan<T>(p, length); }

    // ZERO: add '==' to Span or make Span/ROSpan partial classes.
    public static bool Equals(this Str8 span, Str8 other)
    {
        if (span.Length != other.Length)
            return false;

        for (var i = 0; i < span.Length; i++)
            if (span[i] != other[i])
                return false;

        return true;
    }
}
