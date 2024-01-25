using System.Runtime.CompilerServices;

namespace System.Runtime.CompilerServices
{
    public sealed class InlineArrayAttribute : Attribute
    {
        public InlineArrayAttribute(int size) { }
    }
}
namespace System.Runtime.InteropServices
{
    public static partial class MemoryMarshal
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static Span<T> CreateSpan<T>(ref T reference, int length) => new Span<T>(Unsafe.AsPointer(ref reference), length);
    }
}   