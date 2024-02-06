// Look ma, no System, no 'new' ...
using Sys;
using static Sys.Console;

static class Tests
{
    public static void Run()
    {
        TestUtf8();
        TestFile();
        TestArena();
        Yes("END"u8, "ALL TESTS PASSED."u8);
    }

    struct CStruct
    {
        public int X;
        public float Y;
        bool b;
        double d;
    }
    static void TestArena()
    {
        const int bufferSize = 10_000;
        Arena ar = new(bufferSize);
        ref var s = ref ar.Alloc<CStruct>();

        Yes("ARENA"u8, "CAN ALLOCATE STRUCTS IN AN ARENA."u8);
        Assert.Equal(0, s.X);
        s.X = 3;
        Assert.Equal(3, s.X);

        Yes("ARENA"u8, "CAN ALLOCATE SPANS OF STRUCT TYPES IN AN ARENA."u8);
        var n = 100;
        var span = ar.AllocSpan<CStruct>(n);
        for (int i = 0; i < n; i++) Assert.Equal(0, span[i].X);

        for (int i = 0; i < n; i++) span[i].X = 3;
        for (int i = 0; i < n; i++) Assert.Equal(3, span[i].X);

        Yes("ARENA"u8, "CAN ALLOCATE SPANS OF PRIMITIVE TYPES IN AN ARENA."u8);
        var ispan = ar.AllocSpan<int>(10);
        var fspan = ar.AllocSpan<float>(10);
        var dspan = ar.AllocSpan<double>(10);
        var bspan = ar.AllocSpan<bool>(10);

        for (int i = 0; i < 10; i++)
        {
            Assert.Equal(0, ispan[i]);
            Assert.Equal(0, fspan[i]);
            Assert.Equal(0, dspan[i]);
            Assert.False(bspan[i]);

            ispan[i] = 3;
            fspan[i] = 3.0f;
            dspan[i] = 3.0;
            bspan[i] = true;

            Assert.Equal(3, ispan[i]);
            Assert.Equal(3.0, fspan[i]);
            Assert.Equal(3.0, dspan[i]);
            Assert.True(bspan[i]);
        }

        Yes("ARENA"u8, "CAN ALIGN ON CACHE LINE IN ARENA."u8);
        var cacheLine = ar.AllocSpan<double>(10, alignment: 64);
        unsafe {
            Assert.Equal(0,
                (long)System.Runtime.CompilerServices.Unsafe.AsPointer(ref cacheLine[0]) % 64);
        }

        Yes("ARENA"u8, "CAN ALLOCATE SPANS ON STACK IN ARENA."u8);
        System.Span<byte> buf = stackalloc byte[100];
        var ar1 = new Arena(buf);
        var k = ar1.AllocSpan<double>(2);
        Assert.Equal(0, k[0]);
        k[0] = 3;
        Assert.Equal(2, k.Length);
        Assert.Equal(3, k[0]);

        Yes("ARENA"u8, "CAN RESET ARENA."u8);
        ar.AllocSpan<CStruct>(20);
        Assert.True(ar.BytesLeft < ar.TotalBytes);
        ar.Reset();

        Assert.Equal(bufferSize, (int)ar.BytesLeft);
        var spank = ar.AllocSpan<CStruct>(20);
        Assert.Equal(0, spank[0].X);

        Yes("ARENA"u8, "CAN USE POINTERS TO STRUCTS IN ARENAS."u8);
        ref var ss = ref ar.Alloc<CStruct>();
        ss.X = 3;
        var sp = ar.ToPtr(ref ss);
        ref var sss = ref ar.ToRef<CStruct>(sp);
        Assert.Equal(sss.X, ss.X);
    }

    static void TestFile()
    {

        Yes("FILE"u8, "CAN FLUSH AND SLURP A FILE."u8);
        var filename = "test.txt"u8;
        var msg = "Hello from bflat!"u8;

        File.Flush(filename, msg);
        Yes("FILE"u8, "Wrote message to file."u8);

        var text = File.Slurp(filename, Statics.InFile);
        if(msg.Equals(text)) Yes("FILE"u8, "Read same message from file."u8);
        else No("FILE"u8, "ERROR: different message??"u8);

    }
    static void TestUtf8()
    {
        if(Environment.Argc > 0) Yes("UTF8"u8, "CAN ACCESS ARGS AS UTF8 AND PRINT THEM."u8);
        for (var i = 0; i < Environment.Argc; i++) {
            var a = Environment.Arg(i);
            Yes("UTF8"u8, a);
        }

        No("UTF8"u8, "CANNOT CREATE NON ASCII UTF8 LITERAL STRINGS."u8);
        var u8 = "??"u8; // This creates garbage bytes. 
        Str8 u8s = [0xcf, 0x84, 0xce, 0xb1]; // This works: same string.

        No ("UTF8"u8, u8);
        Yes("UTF8"u8, u8s);

        Yes("UTF8"u8, "CAN CREATE AND PRINT ASCII UTF8 LITERAL STRINGS."u8);
        var s1 = "A string"u8;
        Yes("UTF8"u8, s1);
    }
    static void Yes(Str8 area, Str8 msg) { Write("[+] "u8); Write(area); Write(" - "u8); WriteLine(msg);}
    static void No(Str8 area, Str8 msg) { Write("[-] "u8); Write(area);  Write(" - "u8); WriteLine(msg);}

    // It seems that I cannot create an automatic buffer for the file, so I need to create a static one.
    // ZERO: should be fixed, I think.
    static class Statics
    {
        public static Buffers.K8_Buffer<byte> InFile; 
    }
}
