using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sys;

namespace System
{
    public static partial class SpanUtils
    {
        // Workaround for lack of implicit conversions from Span<T> to ReadOnlySpan<T>.
        // ZERO: add the above to Span or make Span/ROSpan partial classes.
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this Span<T> span) => MemoryMarshal.CreateReadOnlySpan(ref span[0], span.Length);
    }
    // Printing Utf8 directly
    public static unsafe partial class Console
    {
        // Setting the console page on windows to UTF8. The second call is unlikely to work today, but it is there for the future.
        // TODO: should save the old code page and restore it on exit, but whatever, let's bully the user into sanity.
        public static void SetUtf8()
        {
        #if WINDOWS

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleOutputCP(uint wCodePageID);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCP(uint wCodePageID);

        SetConsoleOutputCP(65001); // Set output code page to UTF-8
        SetConsoleCP(65001);       // Set input code page to UTF-8
        #endif
        }

        // Again, lack of implicit Span -> ROSPan conversion.
        public static void WriteLine(ReadOnlySpan<byte> s) => Libc.Puts(s);
        public static void WriteLine(Span<byte> s) => Libc.Puts(s);
    }
}

namespace System.Runtime.CompilerServices
{
    // ZERO: add the below.
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
        // I believe the method below is needed by the compiler for the C# inline array feature, but I don't remember exactly.
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


        // It is the only 'hook point' to get code to execute at the start of the process (i.e., to set the console code page).
        static StartupCodeHelpers() => System.Console.SetUtf8();

        public static string[] GetArgs() => GetMainMethodArguments();
    }
}