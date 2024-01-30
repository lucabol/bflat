using System;
using System.Runtime.InteropServices;

namespace Sys;

unsafe public static class Libc
{
    #if WINDOWS
        const string libc = "msvcrt";
        // I can't make ucrtbase.dll work, so I'm using msvcrt.dll instead.
        //const string libc = "ucrtbase.dll";
        //const string libc = "api-ms-win-crt-stdio-l1-1-0";
    #else
        const string libc = "libc";
    #endif

    [DllImport(libc, CallingConvention = CallingConvention.Cdecl), SuppressGCTransition]
    private static extern IntPtr fopen(byte* filename, byte* mode);

    [DllImport(libc, CallingConvention = CallingConvention.Cdecl), SuppressGCTransition]
    private static extern int fclose(IntPtr stream);

    [DllImport(libc, CallingConvention = CallingConvention.Cdecl), SuppressGCTransition]
    private static extern int fflush(IntPtr stream);

    [DllImport(libc, CallingConvention = CallingConvention.Cdecl), SuppressGCTransition]
    private static extern int printf(byte* format, int arg, byte* arg2);

    [DllImport(libc, CallingConvention = CallingConvention.Cdecl), SuppressGCTransition]
    private static extern unsafe int fread(byte* ptr, int size, int count, IntPtr stream);

    [DllImport(libc, CallingConvention = CallingConvention.Cdecl), SuppressGCTransition]
    private static extern int feof(IntPtr stream);
    
    [DllImport(libc, CallingConvention = CallingConvention.Cdecl), SuppressGCTransition]
    private static extern int ferror(IntPtr stream);

    public unsafe static int Puts(Str8 s)
    {
        // The span might not be zero terminated, so we need to pass the length.
        // For how to pass a dynamic length string to printf,
        // see: https://stackoverflow.com/questions/2239519/is-there-a-way-to-specify-how-many-characters-of-a-string-to-print-out-using-pri
        // TODO: this if statement is a hack, we should fix this properly.
        if(s.Length == 0)
            fixed (byte* format = &("\n"u8)[0])
                return printf(format,0,(byte*)0);

        fixed (byte* b = &s[0], format = &("%.*s\n"u8)[0])
            return printf(format, s.Length, b);
    }
    public static int Puts(Buf8 s) => Puts(s.AsReadOnlySpan());

    public unsafe static IntPtr FOpen(Str8 filename, Str8 mode)
    {
        fixed (byte* filenamePtr = &filename[0], modePtr = &mode[0])
            return fopen(filenamePtr, modePtr);
    }

    public unsafe static int FRead(Buf8 buf, int size, int count, IntPtr stream)
    {
        fixed (byte* ptr = &buf[0])
            return fread(ptr, size, count, stream);
    }

    public static int FClose(IntPtr stream) => fclose(stream);
    public static int FEof  (IntPtr stream) => feof(stream);
    public static int FError(IntPtr stream) => ferror(stream);
}