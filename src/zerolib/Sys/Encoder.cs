using System;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Sys;

public static class Encoder
{
    public unsafe static Span<byte> EncodeToUtf8(string str, Span<byte> buffer)
    {
        var index = 0;

        for(var i = 0; i < str.Length; i++)
        {
            var c = str[i];
            if (c <= 0x7F)
            {
                buffer[index++] = ((byte)c);
            }
            else if (c <= 0x7FF)
            {
                buffer[index++] = ((byte)(0xC0 | (c >> 6)));
                buffer[index++] = ((byte)(0x80 | (c & 0x3F)));
            }
            else if (c <= 0xFFFF)
            {
                buffer[index++] = ((byte)(0xE0 | (c >> 12)));
                buffer[index++] = ((byte)(0x80 | ((c >> 6) & 0x3F)));
                buffer[index++] = ((byte)(0x80 | (c & 0x3F)));
            }
            else
            {
                buffer[index++] = ((byte)(0xF0 | (c >> 18)));
                buffer[index++] = ((byte)(0x80 | ((c >> 12) & 0x3F)));
                buffer[index++] = ((byte)(0x80 | ((c >> 6) & 0x3F)));
                buffer[index++] = ((byte)(0x80 | (c & 0x3F)));
            }
        }

        return new Span<byte>(Unsafe.AsPointer(ref buffer[0]), index);
    }
}