using System;
using Internal.Runtime.CompilerHelpers;

namespace Sys;

public static partial class Env
{
    public static ReadOnlySpan<byte> Arg(int i) => StartupCodeHelpers.GetCommandLineArg(i);
}