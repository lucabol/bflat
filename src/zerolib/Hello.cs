using System;
using System.Runtime.InteropServices;
using static System.Console;

using Sys;

for(var i = 0; i < args.Length; i++) {
    var s16  = args[i];
    var s8  = Encoder.EncodeToUtf8(s16);
    Libc.Puts(s8);
}

var s = "Bob"u8;
Libc.Puts(s);
WriteLine("After puts");