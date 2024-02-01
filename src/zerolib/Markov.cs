using Sys;

// From the Practice of Programming, Kernighan and Pike.
// https://www.cs.princeton.edu/~bwk/tpop.webpage/markov.c

public static class MarkovGenerator
{
    struct Mem
    {
        public int Start;
        public int End;
    }

    const int NPREF = 2; // number of prefix words
    [System.Runtime.CompilerServices.InlineArray(NPREF)]
    struct Word_Tuple { private Mem _e0; }

    struct Prefix
    {
        public Word_Tuple  PrefixWords;
        public int Next;
        public int FirstSuffix;
    }
    struct Suffix
    {
        public Mem SuffixText;
        public int Next;
    }

    // Alternatively, declare the buffers here with a size defined by a constant
    // as done above for the Word_Buffer. That avoids keeping the constants in sync
    // at the expense of ugliness.
    /* KEEP THE CONSTANTS IN SYNC WITH THE BUFFER SIZES BELOW. */
    const int NHASH     = Buffers.K16;

    static Buffers.K16_Buffer<int>     Hashes;
    static Buffers.HugeBuffer<Prefix>  Prefixes;
    static Buffers.HugeBuffer<Suffix>  Suffixes;
    static Buffers.M8_Buffer           Text; // Making this one a HugeBuffer of byte causes a compiler error ...
    static int TextLength;

    static int PrefixNext = 1; // We use 0 as a sentinel value for empty prefixes.
    static int SuffixNext;

    public static void Run(Str8 path, int nwords)
    {
        var txt = File.Slurp(path, Text.Span);
        TextLength = txt.Length;

        Build();
        Generate(nwords);
        //Debug();
    }

    static void Generate(int nwords)
    {
        var seed    = (uint)System.Environment.TickCount64;
        Random rnd  = new (seed);
        var prefix  = (int)rnd.Next() % PrefixNext;

        for(var i = 0; i < nwords; i++)
        {
            var sidx = Prefixes[prefix].FirstSuffix;
            if(sidx == 0)
                Environment.Fail("Prefix without suffix"u8);

            // Count the number of suffixes
            var ns = 0;
            while(sidx != 0)
            {
                ns++;
                sidx = Suffixes[sidx].Next;
            }

            // Pick a random one
            var idx = rnd.Next() % ns;
            sidx = Prefixes[prefix].FirstSuffix;
            while(idx > 0)
            {
                sidx = Suffixes[sidx].Next;
                idx--;
            }

            var suffix = Suffixes[sidx].SuffixText;
            var word = MemToStr(suffix);
            Console.Write(word);
            Console.Write(" "u8);

            Word_Tuple newPrefix = default;
            newPrefix[0] = Prefixes[prefix].PrefixWords[1];
            newPrefix[1] = suffix;
            prefix = Lookup(newPrefix, false);
            if(prefix == 0)
                Environment.Fail("No prefix with these two words"u8);
        }
        Console.WriteLine(""u8);
    }
    
    static void Debug()
    {
        for(var i = 0; i < Prefixes.Length; i++)
        {
            var p = Prefixes[i];
            if(IsPrefixEmpty(ref p))
                continue;

            for(var j = 0; j < NPREF; j++)
            {
                var w = MemToStr(p.PrefixWords[j]);
                Console.Write(w);
                Console.Write(" "u8);
            }
            Console.Write(" -> "u8);
            
            var sidx = p.FirstSuffix;
            while(sidx != 0) { 
                var s = Suffixes[sidx];
                Console.Write(MemToStr(s.SuffixText));
                sidx = s.Next;
            }
            Console.WriteLine(""u8);
        }
        Console.Write("PrefixNext: "u8); System.Console.WriteLine(PrefixNext);
        Console.Write("SuffixNext: "u8); System.Console.WriteLine(SuffixNext);
    }
    static Str8 MemToStr(Mem mem)
    {
        var buf = Text.Span.Slice(mem.Start, mem.End - mem.Start);
        return buf.AsReadOnlySpan();
    }

    static int Hash(Word_Tuple words)
    {
        const int MULTIPLIER = 31;
        var h = 0u;
        for(var w = 0; w < NPREF; w++) {
            var str = MemToStr(words[w]);
            for (var i = 0; i < str.Length; i++)
                h = h * MULTIPLIER + str[i];
        }
        return (int)(h % NHASH);
    }

    static bool IsPrefixEmpty(ref Prefix p) =>
        p.PrefixWords[0].Start == 0 && p.PrefixWords[0].End == 0;

    static int Lookup(Word_Tuple words, bool create)
    {
        var h1 = Hash(words);
        var h  = Hashes[h1];
        var sp = h;

        while(sp != 0)
        {
            ref var p = ref Prefixes[sp];
            int i;
            for (i = 0; i < NPREF; i++)
            {
                var w = MemToStr(words[i]);
                if (!w.Equals(MemToStr(p.PrefixWords[i])))
                    break;
            }

            if(i == NPREF)
                return sp;
            sp = p.Next;
        }

        if(create)
        {
            sp = PrefixNext;
            ref var p = ref Prefixes[sp];
            p.PrefixWords = words;
            p.Next = h;
            Hashes[h1] = sp;

            if(PrefixNext >= Prefixes.Length - 1)
                Environment.Fail("ERROR: OOM Prefix buffer full"u8);

            PrefixNext++;
        }
        return sp;
    }
    static void AddSuffix(int prefix, Mem suffix)
    {
        ref var s = ref Suffixes[SuffixNext];
        s.SuffixText = suffix;

        ref var p = ref Prefixes[prefix];
        s.Next = p.FirstSuffix;
       
        p.FirstSuffix = SuffixNext;

        if(SuffixNext >= Suffixes.Length - 1)
            Environment.Fail("ERROR: OOM Suffix buffer full"u8);

        SuffixNext++;
    }
        
    static void Add(Word_Tuple words, Mem suffix)
    {
        var prefix = Lookup(words, true);
        /*
        var m0 = Prefixes[prefix].PrefixWords[0]; Console.Write(MemToStr(m0)); Console.Write(" "u8);
        var m1 = Prefixes[prefix].PrefixWords[1]; Console.Write(MemToStr(m1)); Console.Write(" => "u8);
        var s = MemToStr(suffix); Console.Write(s); Console.WriteLine(""u8);
        System.Console.WriteLine(SuffixNext);
        */
        AddSuffix(prefix, suffix);
    }
    
    static Mem GetWord(int start)
    {
        // Skip initial spaces
        while(start < TextLength && Text.Span[start] == ' ')
            start++;

        var end = start;
        while(end < TextLength && Text.Span[end] != ' ')
            end++;
        return new Mem { Start = start, End = end };
    }

    static void Build()
    {
        var idx = 0;
        Word_Tuple prefix = default;

        // Fill out the prefix buffer with the first words in the text.
        for(var w = 0; w < NPREF; w++)
        {
           var word = GetWord(idx);
           if(word.Start == word.End)
               return;
            prefix[w] = word;
            idx = word.End;
        }

        while(true) {
            var suffix = GetWord(idx);
            if(suffix.Start == suffix.End)
                return;
            Add(prefix, suffix);
            idx = suffix.End;

            for(var w = 0; w < NPREF - 1; w++)
                prefix[w] = prefix[w + 1];
            prefix[NPREF - 1] = suffix;
        }
    }
}
