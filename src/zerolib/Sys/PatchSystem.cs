using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System
{
    public static partial class SpanUtils
    {
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this Span<T> span) => MemoryMarshal.CreateReadOnlySpan(ref span[0], span.Length);
    }
}

namespace System.Runtime.CompilerServices
{
    // This is needed for the C# inline array feature
    public sealed class InlineArrayAttribute : Attribute
    {
        public InlineArrayAttribute(int size) { }
    }
    // This is needed for the C# extension method feature
    public sealed class ExtensionAttribute : Attribute
    {
        public ExtensionAttribute() { }
    }
}

namespace System.Runtime.InteropServices
{
    public static partial class MemoryMarshal
    {
        // I belive the method below is needed by the compiler for the C# inline array feature, but I don't remember exactly.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static Span<T> CreateSpan<T>(ref T reference, int length) => new Span<T>(Unsafe.AsPointer(ref reference), length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static ReadOnlySpan<T> CreateReadOnlySpan<T>(ref T reference, int length) => new ReadOnlySpan<T>(Unsafe.AsPointer(ref reference), length);
    }
}

namespace Internal.Runtime.CompilerHelpers
{
    // This is needed to expose an uniform way to get to the command line arguments without having them passed in as a parameter to UTF8 conversion.
    unsafe partial class StartupCodeHelpers {
        public static string[] GetArgs() => GetMainMethodArguments();
    }
}