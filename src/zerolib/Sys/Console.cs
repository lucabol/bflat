namespace Sys;

public static partial class Console
{
    // Lack of implicit Span -> ROSPan conversion.
    public static void WriteLine(Str8 s) => Libc.Puts(s);
    public static void WriteLine(Buf8 s) => Libc.Puts(s);

    public static void Write(Str8 s) => Libc.Put(s);
    public static void Write(Buf8 s) => Libc.Put(s);
}