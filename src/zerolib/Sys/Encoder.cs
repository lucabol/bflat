using System;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Sys;

public static class Encoder
{
    // https://stackoverflow.com/questions/9115279/commandline-argument-parameter-limitation
    const int MAX_COMMAND_LINE_LENGTH = 8192;

    [System.Runtime.CompilerServices.InlineArray(MAX_COMMAND_LINE_LENGTH)]
    struct Buffer { private byte _element0; }
    static Buffer _commandLine;
    static int _index = 0;

    // This is what Github Copilot suggested, slightly modified to use a string pool and to avoid allocations.
    public unsafe static ReadOnlySpan<byte> EncodeToUtf8(string str)
    {
        int start = _index;        

        for(var i = 0; i < str.Length; i++)
        {
            var c = str[i];
            if (c <= 0x7F)
            {
                _commandLine[_index++] = ((byte)c);
            }
            else if (c <= 0x7FF)
            {
                _commandLine[_index++] = ((byte)(0xC0 | (c >> 6)));
                _commandLine[_index++] = ((byte)(0x80 | (c & 0x3F)));
            }
            else if (c <= 0xFFFF)
            {
                _commandLine[_index++] = ((byte)(0xE0 | (c >> 12)));
                _commandLine[_index++] = ((byte)(0x80 | ((c >> 6) & 0x3F)));
                _commandLine[_index++] = ((byte)(0x80 | (c & 0x3F)));
            }
            else
            {
                _commandLine[_index++] = ((byte)(0xF0 | (c >> 18)));
                _commandLine[_index++] = ((byte)(0x80 | ((c >> 12) & 0x3F)));
                _commandLine[_index++] = ((byte)(0x80 | ((c >> 6) & 0x3F)));
                _commandLine[_index++] = ((byte)(0x80 | (c & 0x3F)));
            }
        }

        var len = _index - start;
        var cmd = _commandLine[0];
        return new ReadOnlySpan<byte>(Unsafe.AsPointer(ref _commandLine[0]), len);
    }
}