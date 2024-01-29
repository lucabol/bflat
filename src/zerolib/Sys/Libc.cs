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
    private static extern int puts(byte* s);

    [DllImport(libc, CallingConvention = CallingConvention.Cdecl), SuppressGCTransition]
    private static extern IntPtr fopen(byte* filename, byte* mode);

    [DllImport(libc, CallingConvention = CallingConvention.Cdecl), SuppressGCTransition]
    private static extern int fclose(IntPtr stream);

    [DllImport(libc, CallingConvention = CallingConvention.Cdecl), SuppressGCTransition]
    private static extern int fputc(int c, IntPtr stream);

    [DllImport(libc, CallingConvention = CallingConvention.Cdecl), SuppressGCTransition]
    private static extern int fputs(byte* str, IntPtr stream);

    [DllImport(libc, CallingConvention = CallingConvention.Cdecl), SuppressGCTransition]
    private static extern int fgetc(IntPtr stream);

    [DllImport(libc, CallingConvention = CallingConvention.Cdecl), SuppressGCTransition]
    private static extern IntPtr fgets(byte* str, int num, IntPtr stream);

    [DllImport(libc, CallingConvention = CallingConvention.Cdecl), SuppressGCTransition]
    private static extern int feof(IntPtr stream);

    [DllImport(libc, CallingConvention = CallingConvention.Cdecl), SuppressGCTransition]
    private static extern int fseek(IntPtr stream, long offset, int origin);

    [DllImport(libc, CallingConvention = CallingConvention.Cdecl), SuppressGCTransition]
    private static extern long ftell(IntPtr stream);

    [DllImport(libc, CallingConvention = CallingConvention.Cdecl), SuppressGCTransition]
    private static extern int fflush(IntPtr stream);

    [DllImport(libc, CallingConvention = CallingConvention.Cdecl), SuppressGCTransition]
    private static extern int printf(byte* format, int arg, byte* arg2);

    [DllImport(libc, CallingConvention = CallingConvention.Cdecl), SuppressGCTransition]
    private static extern int strlen(byte* s);

    public static int Strlen(char* wcs)
    {
        var len = strlen((byte*) wcs);
        return len * 2;
    }

    public static int Puts(ReadOnlySpan<byte> s)
    {
        unsafe
        {
            // The span might not be zero terminated, so we need to pass the length.
            // For how to pass a dynamic length string to printf,
            // see: https://stackoverflow.com/questions/2239519/is-there-a-way-to-specify-how-many-characters-of-a-string-to-print-out-using-pri
            fixed (byte* b = &s[0], format = &("%.*s\n"u8)[0])
                return printf(format, s.Length, b);
        }
    }

    public static IntPtr FOpen(ReadOnlySpan<byte> filename, ReadOnlySpan<byte> mode)
    {
        unsafe
        {
            fixed (byte* filenamePtr = &filename[0])
            fixed (byte* modePtr = &mode[0])
            {
                return fopen(filenamePtr, modePtr);
            }
        }
    }

    public static int FClose(IntPtr stream)
    {
        return fclose(stream);
    }

    public static int FPutc(int c, IntPtr stream)
    {
        return fputc(c, stream);
    }

    public static int FPuts(ReadOnlySpan<byte> str, IntPtr stream)
    {
        unsafe
        {
            fixed (byte* strPtr = &str[0])
            {
                return fputs(strPtr, stream);
            }
        }
    }

    public static int FGetc(IntPtr stream)
    {
        return fgetc(stream);
    }

    public static IntPtr FGets(ReadOnlySpan<byte> str, int num, IntPtr stream)
    {
        unsafe
        {
            fixed (byte* strPtr = &str[0])
            {
                return fgets(strPtr, num, stream);
            }
        }
    }

    public static int Feof(IntPtr stream)
    {
        return feof(stream);
    }

    public static int FSeek(IntPtr stream, long offset, int origin)
    {
        return fseek(stream, offset, origin);
    }

    public static long FTell(IntPtr stream)
    {
        return ftell(stream);
    }

    public static int FFlush(IntPtr stream)
    {
        return fflush(stream);
    }


}