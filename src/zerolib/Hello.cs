using System;
using System.Runtime.InteropServices;
using static System.Console;

using Sys;

var buffer = new byte[1024];
var argss = Internal.Runtime.CompilerHelpers.StartupCodeHelpers.GetArgs();

for(var i = 0; i < argss.Length; i++) {
    var s16  = argss[i];
    var s8  = Encoder.EncodeToUtf8(s16, new Span<byte>(buffer, 0, 1024));
    Libc.Puts(s8.AsReadOnlySpan());
}

var s = "Bob"u8;
Libc.Puts(s);
WriteLine("After puts");