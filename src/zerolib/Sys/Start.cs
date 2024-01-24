using System;
using System.Runtime;
using System.Runtime.InteropServices;

namespace Internal.Runtime.CompilerHelpers;

unsafe partial class StartupCodeHelpers
{
    // https://stackoverflow.com/questions/9115279/commandline-argument-parameter-limitation
    const int MAX_COMMAND_LINE_LENGTH = 8192;
    static byte[] _commandLine = new byte[MAX_COMMAND_LINE_LENGTH];
    static int _index = 0;

    // This is what Github Copilot suggested, slightly modified to use a string pool and to avoid allocations.
    static ReadOnlySpan<byte> EncodeToUtf8(string str)
    {
        var start = _index;        

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

        return new ReadOnlySpan<byte>(_commandLine, start, _index - start);
    }

    public static ReadOnlySpan<byte> GetCommandLineArg(int i)
    {

    #if LINUX
    #endif

    #if WINDOWS

        int argc;
        char** argv = CommandLineToArgvW(GetCommandLineW(), &argc);

        [DllImport("kernel32"), SuppressGCTransition]
        static extern char* GetCommandLineW();

        [DllImport("shell32"), SuppressGCTransition]
        static extern char** CommandLineToArgvW(char* lpCmdLine, int* pNumArgs);

        var args = GetMainMethodArguments();
        return EncodeToUtf8(args[i]);
    #endif
    }
}