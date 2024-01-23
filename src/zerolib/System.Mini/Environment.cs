using System;
using Internal.Runtime.CompilerHelpers;

namespace System;

public static partial class Environment
{
    public static ReadOnlySpan<byte> GetCommandLineArg(int i) => StartupCodeHelpers.GetCommandLineArg(i);
}