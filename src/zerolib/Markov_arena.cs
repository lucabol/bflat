using Sys;

// From the Practice of Programming, Kernighan and Pike.
// https://www.cs.princeton.edu/~bwk/tpop.webpage/markov.c
// See Markov_noalloc.cs for more detailed comments on the algorithms.
// Here I just comment on memory management.

public static class MarkovArenaGenerator
{
    struct Mem
    {
        public int Start;
        public int End;
    }

    const int NPREF = 2;
    [System.Runtime.CompilerServices.InlineArray(NPREF)]
    struct Word_Tuple { private Mem _e0; }

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

    // Maximum size of the memory arena and of the text file to load in memory.
    const int MAXMEM  = 1024 * 1024 * 128;
    const int MAXTEXT = 1024 * 1024 * 5;

    // This is the same size that the hashtable ends up being for the kjbible.txt file in the standard implementation.
    // Aka, .NET's Dictionary<T> resizes itself to this number at that file size.
    // Think of it as manual `calibration` of the implementation for the specific max size.
    const int NHASH   = 300_000;

    // In this implementation, pointers are just indexes into the arena.
    // Pointer to the hashtable of prefixes.
    static nint hash;
   
    public static void Run(Str8 path, int nwords)
    {
        Arena ar = new(MAXMEM);

        System.Span<byte> text = ar.AllocSpan<byte>(MAXTEXT);
        var s = File.Slurp(path, text);

        // I could pass the pointer to the hashtable to the functions below.
        // Not clear why I am passing most things, but not all of them.
        var h = ar.AllocSpan<nint>(NHASH);
        (hash, _) = ar.ToPtrAndLength(h);

        // This is a trickiness of working with `ref struct`.
        // If you don't pass it by ref, the compiler will copy it.
        // And the copy will be modified, not the original, causing hard to detect bugs.
        // Given that, the Arena should probably not be a `ref struct`.
        Build(ref ar, s);
        Generate(ref ar, s, nwords);
    }

    static void Generate(ref Arena ar, Str8 text, int nwords)
    {
        var seed    = (uint)System.Environment.TickCount64;
        Random rnd  = new (seed);
        var prefix  = (int)rnd.Next() % NHASH;

        // This is how you go from a pointer to an entity.
        var hashTbl = ar.ToSpan<nint>(hash, NHASH);
        while(hashTbl[prefix] == 0 && prefix < NHASH)
            prefix++;

        if(prefix == NHASH)
            Environment.Fail("No non-empty prefix"u8);

        // Rehidrate the prefix from the pointer.
        // The `ref` syntax is heavy for these operations.
        ref var pre = ref ar.ToRef<Prefix>(hashTbl[prefix]);

        for(var i = 0; i < nwords; i++)
        {
            var sidx = pre.FirstSuffix;
            if(sidx == 0)
                Environment.Fail("Prefix without suffix"u8);

            var ns = 0;
            while(sidx != 0)
            {
                ns++;
                sidx = ar.ToRef<Suffix>(sidx).Next;
            }

            var idx = rnd.Next() % ns;
            sidx = pre.FirstSuffix;
            while(idx > 0)
            {
                sidx = ar.ToRef<Suffix>(sidx).Next;
                idx--;
            }

            var suffix = ar.ToRef<Suffix>(sidx).SuffixText;
            var word = MemToStr(text, suffix);
            Console.Write(word);
            Console.Write(" "u8);

            Word_Tuple newPrefix = default;
            newPrefix[0] = pre.PrefixWords[1];
            newPrefix[1] = suffix;

            pre = Lookup(ref ar, text, newPrefix, false);
            if(IsPrefixEmpty(ref pre))
                Environment.Fail("No prefix with these two words"u8);
        }
        Console.WriteLine(""u8);
    }

    static Str8 MemToStr(Str8 text, Mem mem)
    {
        var buf = text.Slice(mem.Start, mem.End - mem.Start);
        return buf;
    }
    
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

    static ref Prefix Lookup(ref Arena ar, Str8 text, Word_Tuple words, bool create)
    {
        var hashTbl = ar.ToSpan<nint>(hash, NHASH);

        var h1 = Hash(text, words);

        var h  = hashTbl[h1];
        var sp = h;

        ref var p = ref ar.ToRef<Prefix>(sp);

        while(sp != 0)
        {
            p = ref ar.ToRef<Prefix>(sp);
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
            p = ref ar.Alloc<Prefix>();

            p.PrefixWords = words;
            p.FirstSuffix = 0;

            p.Next = hashTbl[h1];
            hashTbl[h1] = ar.ToPtr(ref p);
        }
        return ref p;
    }

    static void AddSuffix(ref Arena ar, ref Prefix p, Mem suffix)
    {
        ref var s = ref ar.Alloc<Suffix>();
        s.SuffixText = suffix;
        s.Next = p.FirstSuffix;
       
        p.FirstSuffix = ar.ToPtr(ref s);
    }

    static void Add(ref Arena ar, Str8 text, Word_Tuple words, Mem suffix)
    {
        ref var prefix = ref Lookup(ref ar, text, words, true);
        AddSuffix(ref ar, ref prefix, suffix);
    }
    
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

    static void Build(ref Arena ar, Str8 text)
    {
        var idx = 0;
        Word_Tuple prefix = default;


        for(var w = 0; w < NPREF; w++)
        {
           var word = GetWord(text, idx);
           if(word.Start == word.End)
               return;
            prefix[w] = word;
            idx = word.End;
        }

        while(true) {
            var suffix = GetWord(text, idx);
            if(suffix.Start == suffix.End)
                return;

            Add(ref ar, text, prefix, suffix);
            idx = suffix.End;

            for(var w = 0; w < NPREF - 1; w++)
                prefix[w] = prefix[w + 1];
            prefix[NPREF - 1] = suffix;
        }
    }
}
