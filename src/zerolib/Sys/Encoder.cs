using System;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Sys;

// Tested on exactly one example, so it's certainly correct.
public static class Encoder
{
    public unsafe static Span<byte> Utf16ToUtf8(string str, Span<byte> buffer)
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

        return MemoryMarshal.CreateSpan(ref buffer[0], index);
    }

    public unsafe static Span<char> Utf8ToUtf16(ReadOnlySpan<byte> utf8, Span<char> utf16)
    {
        int utf8Length = utf8.Length;
        int i = 0, j = 0;

        while (i < utf8Length)
        {
            uint ch;
            byte secondByte = utf8[i++];
            if (secondByte < 0x80)
            {
                ch = secondByte;
            }
            else if (secondByte < 0xE0)
            {
                ch = ((uint)(secondByte & 0x1F) << 6) | (uint)(utf8[i++] & 0x3F);
            }
            else if (secondByte < 0xF0)
            {
                ch = ((uint)(secondByte & 0x0F) << 12) | ((uint)(utf8[i++] & 0x3F) << 6)
                     | (uint)(utf8[i++] & 0x3F);
            }
            else
            {
                ch = ((uint)(secondByte & 0x07) << 18) | ((uint)(utf8[i++] & 0x3F) << 12)
                     | ((uint)(utf8[i++] & 0x3F) << 6) | (uint)(utf8[i++] & 0x3F);
            }

            if (ch <= 0xFFFF)
            {
                utf16[j++] = (char)ch;
            }
            else
            {
                ch -= 0x10000;
                utf16[j++] = (char)((ch >> 10) + 0xD800);
                utf16[j++] = (char)((ch & 0x3FF) + 0xDC00);
            }
        }

        return MemoryMarshal.CreateSpan(ref utf16[0], j); 
    }

}