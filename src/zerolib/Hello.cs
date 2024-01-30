using System;
using static System.Console;

using Sys;

if(args.Length > 0) WriteLine("[+] CAN ACCESS ARGS AS UTF8 AND PRINT THEM."u8);
for (var i = 0; i < args.Length; i++) {
    var a = Env.Arg(i);
    WriteLine(a);
}
WriteLine(""u8);

WriteLine("[-] CANNOT CREATE NON ASCII UTF8 LITERAL STRINGS."u8);
var u8 = "??"u8; // This creates garbage bytes. 
ReadOnlySpan<byte> u8s = [0xe4, 0xbd, 0xa0, 0xe5, 0xa5, 0xbd, 0x0a]; // This works: same string.

WriteLine(u8);
WriteLine(u8s);

WriteLine("[+] CAN CREATE AND PRINT ASCII UTF8 LITERAL STRINGS."u8);
var s1 = "A string"u8;
WriteLine(s1);
