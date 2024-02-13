using Sys;

// From the Practice of Programming, Kernighan and Pike.
// https://www.cs.princeton.edu/~bwk/tpop.webpage/markov.c

public static class MarkovNoAllocGenerator
{
    // This represents a single word in the Text buffer.
    // TODO: this wasted bits. The length of a word is less than 255. Also, the bible is 4.5MB.
    // The whole structure could be packed into 4 bytes, dramatically increasing cache utilization.
    struct Mem
    {
        public int Start;
        public int End;
    }

    // To keep the genericity of the original implementation, we allow different number of prefix words.
    // Hardcoding the number would allow more optimizations.
    const int NPREF = 2; // number of prefix words
    [System.Runtime.CompilerServices.InlineArray(NPREF)]
    struct Word_Tuple { private Mem _e0; }

    // TODO: As above these two structs could be packed more tightly.
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

    // The data structure replicate the original one without the allocations.
    // Aka, an hashtable of prefix words, where each prefix points to a list of suffixes.
    // It keeps the whole text in memory at all times and uses Span to to point to prefixes and suffixes.
    // TODO: Using open addressing and packing the above structures would reduce the memory footprint.
    // TODO: Also, the buffers below are huge. I didn't spend time measuring how big they really need to be.

    /* KEEP THE CONSTANT IN SYNC WITH THE BUFFER SIZE BELOW. */
    const int NHASH     = Buffers.BIGSIZE;
    static Buffers.BigIntBuffer      Hashes;

    static Buffers.HugeBuffer<Prefix>  Prefixes;
    static Buffers.HugeBuffer<Suffix>  Suffixes;
    static Buffers.M8_Buffer           Text; // Making this one a HugeBuffer of byte causes a bflat compiler error ...
    static int TextLength;

    // These point to the next free slots. ++ is like malloc.
    static int PrefixNext;
    static int SuffixNext;

    public static void Run(Str8 path, int nwords)
    {
        // We use 0 as a sentinel value for empty prefixes, but initializing it in the static part of the class brings all statics out of .bss segment.
        PrefixNext = 1;
        var txt = File.Slurp(path, Text.Span);
        TextLength = txt.Length;

        Build();
        Generate(nwords);
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

            // Print the suffix
            var suffix = Suffixes[sidx].SuffixText;
            var word = MemToStr(suffix);
            Console.Write(word);
            Console.Write(" "u8);

            // Rotate the prefix words to the left and add the suffix to the end.
            Word_Tuple newPrefix = default;
            newPrefix[0] = Prefixes[prefix].PrefixWords[1];
            newPrefix[1] = suffix;
            prefix = Lookup(newPrefix, false);
            if(prefix == 0)
                prefix  = (int)rnd.Next() % PrefixNext;
        }
        Console.WriteLine(""u8);
    }
    
    // Prints the data structure and number of items
    // Rehidrate Mem as a proper Str8
    static Str8 MemToStr(Mem mem)
    {
        var buf = Text.Span.Slice(mem.Start, mem.End - mem.Start);
        return buf.AsReadOnlySpan();
    }

    // Same hash function as the original implementation.
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

    static void PrintPrefix(ref Prefix p)
    {
        for(var i = 0; i < NPREF; i++)
        {
            var word = MemToStr(p.PrefixWords[i]);
            Console.Write(word);
            Console.Write(" "u8);
        }
        System.Console.WriteLine("");
    }

    // This follows the original implementation, but uses the next slot in the prefix buffer instead of malloc.
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
            // Allocate a new prefix (i.e., malloc)
            sp = PrefixNext;
            ref var p = ref Prefixes[sp];

            p.PrefixWords = words;
            p.Next = h;
            Hashes[h1] = sp;

            if(PrefixNext >= Prefixes.Length - 1)
                Environment.Fail("ERROR: OOM Prefix buffer full"u8);

            //PrintPrefix(ref p);
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
        AddSuffix(prefix, suffix);
    }
    
    // TODO: perhaps a better text could be generated by using a more sophisticated tokenizer.
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

        // And just keep 1. Getting a new suffix 2. Adding (prefix, suffix) to the data structure 3. Rotating the prefix buffer.
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
