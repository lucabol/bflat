using System;
using Internal.Runtime.CompilerHelpers;
using System.Runtime.InteropServices;

namespace Sys;

/*
 * In the spirit of no-allocation, I need to cache the command line arguments converted to utf8 because they could be accessed multiple times
 * and I cannot 'new' them.
 */

public static partial class Env
{
    // https://stackoverflow.com/questions/9115279/commandline-argument-parameter-limitation
    static Buffers.K8_Buffer<byte> _commandLine;
    static Buffers.K_Buffer<int> _indexes;

    static int _argc = -1;

    static void FillCommandLineCache()
    {
        var args = StartupCodeHelpers.GetArgs();
        _argc = args.Length;
        var index = 0;

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            var buf = MemoryMarshal.CreateSpan(ref _commandLine[index], 1024 * 8);

            var utf8 = Encoder.Utf16ToUtf8(arg, buf);
            _indexes[i] = index;
            index += utf8.Length;
        }
    }

    public static ReadOnlySpan<byte> Arg(int index)
    {

        if (_argc == -1)
            FillCommandLineCache();

        if(index < 0 || index >= _argc)
            Environment.FailFast("Invalid index for command line arguments");

        var start = _indexes[index];
        var end = _indexes[index + 1];
        return MemoryMarshal.CreateReadOnlySpan(ref _commandLine[start], end - start);
    } 
}