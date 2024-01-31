// Look ma, no System, no 'new' ...
using Sys;
using static Sys.Console;

static class Tests
{
    public static void Run()
    {

        if(Environment.Argc > 0) WriteLine("[+] CAN ACCESS ARGS AS UTF8 AND PRINT THEM."u8);
        for (var i = 0; i < Environment.Argc; i++) {
            var a = Environment.Arg(i);
            WriteLine(a);
        }
        WriteLine(""u8);

        WriteLine("[-] CANNOT CREATE NON ASCII UTF8 LITERAL STRINGS."u8);
        var u8 = "??"u8; // This creates garbage bytes. 
        Str8 u8s = [0xe4, 0xbd, 0xa0, 0xe5, 0xa5, 0xbd, 0x0a]; // This works: same string.

        WriteLine(u8);
        WriteLine(u8s);

        WriteLine("[+] CAN CREATE AND PRINT ASCII UTF8 LITERAL STRINGS."u8);
        var s1 = "A string"u8;
        WriteLine(s1);

        WriteLine("\n[+] CAN FLUSH AND SLURP A FILE."u8);
        var filename = "test.txt"u8;
        var msg = "Hello from bflat!"u8;

        File.Flush(filename, msg);
        WriteLine("Wrote message to file."u8);

        var text = File.Slurp(filename, Statics.InFile);
        if(msg.Equals(text)) WriteLine("Read same message from file."u8);
        else WriteLine("ERROR: different message??"u8);

    }
    // It seems that I cannot create an automatic buffer for the file, so I need to create a static one.
    // ZERO: should be fixed, I think.
    static class Statics
    {
        public static Buffers.K8_Buffer<byte> InFile; 
    }
}
