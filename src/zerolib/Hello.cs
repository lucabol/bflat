using System;
using System.Runtime.InteropServices;
using static System.Console;

using Sys;

for(var i = 0; i < args.Length; i++)
    Libc.Puts(Env.Arg(i));

var s = "Bob"u8;
Libc.Puts(s);
WriteLine("After puts");