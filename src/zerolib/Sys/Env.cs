using System;
using Internal.Runtime.CompilerHelpers;
using System.Runtime.InteropServices;

namespace Sys;

public static partial class Env
{
    // https://stackoverflow.com/questions/9115279/commandline-argument-parameter-limitation
    const int MAX_COMMAND_LINE_LENGTH = 8192;
    const int MAX_ARGS = 64;

    // Unsafe fixed arrays would be nice here syntactically, but would be unsafe :-)
    [System.Runtime.CompilerServices.InlineArray(MAX_COMMAND_LINE_LENGTH)]
    struct Buffer { private byte _element0; }
    static Buffer _commandLine;

    [System.Runtime.CompilerServices.InlineArray(MAX_ARGS)]
    struct ArgsIndexes { private int _element0; }
    static ArgsIndexes _indexes;

    static void FillCommandLineCache()
    {
        var args = StartupCodeHelpers.GetArgs();
        var index = 0;

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            var buf = MemoryMarshal.CreateSpan(ref _commandLine[0], MAX_COMMAND_LINE_LENGTH);

            var utf8 = Encoder.EncodeToUtf8(arg, buf);
            _indexes[i] = index;
            index += utf8.Length;
        }
    }
}