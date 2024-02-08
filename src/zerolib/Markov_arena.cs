using Sys;

// From the Practice of Programming, Kernighan and Pike.
// https://www.cs.princeton.edu/~bwk/tpop.webpage/markov.c

public static class MarkovArenaGenerator
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
        public nint Next;
        public nint FirstSuffix;
    }
    struct Suffix
    {
        public Mem SuffixText;
        public nint Next;
    }

    const int MAXMEM  = 1024 * 1024 * 128;
    const int MAXTEXT = 1024 * 1024 * 5;
    const int NHASH   = 4093;

    // Pointer to the hashtable of prefixes.
    static nint hash;
   
    public static void Run(Str8 path, int nwords)
    {
        Arena ar = new(MAXMEM);

        System.Span<byte> text = ar.AllocSpan<byte>(MAXTEXT);
        var s = File.Slurp(path, text);

        var h = ar.AllocSpan<nint>(NHASH);
        (hash, _) = ar.ToPtrAndLength(h);

        Build(ar, s);
        Generate(ar, s, nwords);
    }

    static void Generate(Arena ar, Str8 text, int nwords)
    {
        var seed    = (uint)System.Environment.TickCount64;
        Random rnd  = new (seed);
        var prefix  = (int)rnd.Next() % NHASH;

        // Find the first non-empty prefix after the random one.
        var hashTbl = ar.ToSpan<nint>(hash, NHASH);
        while(hashTbl[prefix] == 0 && prefix < NHASH)
            prefix++;

        if(prefix == NHASH)
            Environment.Fail("No non-empty prefix"u8);

        ref var pre = ref ar.ToRef<Prefix>(hashTbl[prefix]);

        for(var i = 0; i < nwords; i++)
        {
            var sidx = pre.FirstSuffix;
            if(sidx == 0)
                Environment.Fail("Prefix without suffix"u8);

            // Count the number of suffixes
            var ns = 0;
            while(sidx != 0)
            {
                ns++;
                sidx = ar.ToRef<Suffix>(sidx).Next;
            }

            // Pick a random one
            var idx = rnd.Next() % ns;
            sidx = pre.FirstSuffix;
            while(idx > 0)
            {
                sidx = ar.ToRef<Suffix>(sidx).Next;
                idx--;
            }

            // Print the suffix
            var suffix = ar.ToRef<Suffix>(sidx).SuffixText;
            var word = MemToStr(text, suffix);
            Console.Write(word);
            Console.Write(" "u8);

            // Rotate the prefix words to the left and add the suffix to the end.
            Word_Tuple newPrefix = default;
            newPrefix[0] = pre.PrefixWords[1];
            newPrefix[1] = suffix;

            pre = Lookup(ar, text, newPrefix, false);
            if(IsPrefixEmpty(ref pre))
                Environment.Fail("No prefix with these two words"u8);
        }
        Console.WriteLine(""u8);
    }

    // Rehidrate Mem as a proper Str8
    static Str8 MemToStr(Str8 text, Mem mem)
    {
        var buf = text.Slice(mem.Start, mem.End - mem.Start);
        return buf;
    }
    // Same hash function as the original implementation.
    static int Hash(Str8 text, Word_Tuple words)
    {
        const int MULTIPLIER = 31;
        var h = 0u;
        for(var w = 0; w < NPREF; w++) {
            var str = MemToStr(text, words[w]);
            for (var i = 0; i < str.Length; i++)
                h = h * MULTIPLIER + str[i];
        }
        return (int)(h % NHASH);
    }

    static bool IsPrefixEmpty(ref Prefix p) =>
        p.PrefixWords[0].Start == 0 && p.PrefixWords[0].End == 0;

    static void PrintPrefix(ref Prefix p, Str8 text)
    {
        for(var i = 0; i < NPREF; i++)
        {
            var word = MemToStr(text, p.PrefixWords[i]);
            Console.Write(word);
            Console.Write(" "u8);
        }
        Console.WriteLine(""u8);
    }

    // This follows the original implementation, but uses the next slot in the prefix buffer instead of malloc.
    static ref Prefix Lookup(Arena ar, Str8 text, Word_Tuple words, bool create)
    {
        var hashTbl = ar.ToSpan<nint>(hash, NHASH);

        var h1 = Hash(text, words);

        var h  = hashTbl[h1];
        var sp = h;

        while(sp != 0)
        {
            ref var p = ref ar.ToRef<Prefix>(sp);
            int i;

            for (i = 0; i < NPREF; i++)
            {
                var w = MemToStr(text, words[i]);
                if (!w.Equals(MemToStr(text, p.PrefixWords[i])))
                    break;
            }

            if(i == NPREF)
                return ref p;
            sp = p.Next;
        }

        if(create)
        {
            ref var nsp = ref ar.Alloc<Prefix>();

            nsp.PrefixWords = words;
            nsp.FirstSuffix = 0;

            nsp.Next = hashTbl[h1];
            hashTbl[h1] = ar.ToPtr(ref nsp);
            return ref nsp;
        }
        Environment.Fail("No prefix with these two words"u8);

        // This code will never execute
        return ref ar.Alloc<Prefix>();;
    }

    static void AddSuffix(Arena ar, ref Prefix p, Mem suffix)
    {
        ref var s = ref ar.Alloc<Suffix>();
        s.SuffixText = suffix;
        s.Next = ar.ToPtr(ref p.FirstSuffix);
       
        p.FirstSuffix = ar.ToPtr(ref s);
    }

    static void Add(Arena ar, Str8 text, Word_Tuple words, Mem suffix)
    {
        ref var prefix = ref Lookup(ar, text, words, true);
        AddSuffix(ar, ref prefix, suffix);
    }
    
    // TODO: perhaps a better text could be generated by using a more sophisticated tokenizer.
    static Mem GetWord(Str8 text, int start)
    {
        // Skip initial spaces
        while(start < text.Length && text[start] == ' ')
            start++;

        var end = start;
        while(end < text.Length && text[end] != ' ')
            end++;
        return new Mem { Start = start, End = end };
    }

    static void Build(Arena ar, Str8 text)
    {
        var idx = 0;
        Word_Tuple prefix = default;


        // Fill out the prefix buffer with the first words in the text.
        for(var w = 0; w < NPREF; w++)
        {
           var word = GetWord(text, idx);
           if(word.Start == word.End)
               return;
            prefix[w] = word;
            idx = word.End;
        }

        // And just keep 1. Getting a new suffix 2. Adding (prefix, suffix) to the data structure 3. Rotating the prefix buffer.
        while(true) {
            var suffix = GetWord(text, idx);
            if(suffix.Start == suffix.End)
                return;

            Add(ar, text, prefix, suffix);
            idx = suffix.End;

            for(var w = 0; w < NPREF - 1; w++)
                prefix[w] = prefix[w + 1];
            prefix[NPREF - 1] = suffix;
        }
    }
}
