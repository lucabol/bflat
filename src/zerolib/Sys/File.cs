namespace Sys;

public static partial class File
{
    public static Str8 Slurp(Str8 path, Buf8 buf)
    {
        var fd = Libc.FOpen(path, "r"u8);

        if(fd == 0)
            System.Environment.FailFast("File not found");

        var len = Libc.FRead(buf, 1, buf.Length, fd);

        if(Libc.FError(fd) != 0)
            System.Environment.FailFast("Error reading file");
        if(Libc.FEof(fd) == 0)
            System.Environment.FailFast("File too big");

        if(Libc.FClose(fd) != 0)
            System.Environment.FailFast("Error closing file");

        return System.Runtime.InteropServices
            .MemoryMarshal.CreateReadOnlySpan(ref buf[0], len);
    }
}