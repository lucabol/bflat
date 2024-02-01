namespace Sys;

/*
 * In the spirit of no-allocation, I need to cache the command line arguments converted to utf8 because they could be accessed multiple times
 * and I cannot 'new' them. Also, to be consistent with the other APIs in Sys, the user should pass the buffer for the command line arguments.
 * In this case, I use an internal buffer instead because in most programming models, args are ready to use at startup.
 * The buffer could be avoided in Unix as you get the commands in UTF8 format, but not in Windows where they came in as UTF16.
 */

public static partial class Environment
{
    // https://stackoverflow.com/questions/9115279/commandline-argument-parameter-limitation
    static Buffers.K8_Buffer<byte> _commandLine;
    static Buffers.K_Buffer<int> _indexes;

    // -1 means not initialized. Rejoice, we saved one byte for the bool, but wasted some Kbs for the _commandLine buffer.
    static int _argc = -1;

    public static int Argc
    {
        get
        {
            if(_argc == -1)
                FillCommandLineCache();
            return _argc;
        }
    }

    static void FillCommandLineCache()
    {
        var args = Internal.Runtime.CompilerHelpers.StartupCodeHelpers.GetArgs();
        _argc = args.Length;
        var index = 0;

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            var buf = System.Runtime.InteropServices.MemoryMarshal.CreateSpan(ref _commandLine[index], 1024 * 8);

            var utf8 = Encoder.Utf16ToUtf8(arg, buf);
            _indexes[i] = index;
            index += utf8.Length;
        }
    }

    public static Str8 Arg(int index)
    {

        if (_argc == -1)
            FillCommandLineCache();

        if(index < 0 || index >= _argc)
            System.Environment.FailFast("Invalid index for command line arguments");

        var start = _indexes[index];
        var end = _indexes[index + 1];
        return System.Runtime.InteropServices.MemoryMarshal.CreateReadOnlySpan(ref _commandLine[start], end - start);
    }

    // FailFast doesn't print a the message??
    public static void Fail(Str8 message) {
        Console.WriteLine(message);
        System.Environment.FailFast(default);
    }
}