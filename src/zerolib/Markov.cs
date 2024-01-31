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
        public int FirstPostfix;
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
    const int NHASH = 1024 * 4; // size of state hash table array
    static Buffers.K4_Buffer<Prefix>  Prefixes;
    static Buffers.K4_Buffer<Suffix>  Postfixes;
    static Buffers.M8_Buffer          Text;
    static int TextLength;

    static int PrefixNext;
    static int SuffixNext;

    public unsafe static void Run(Str8 path, int nwords)
    {
        File.Slurp(path, Text.Span);
        Build();
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
        var h = Hash(words);
        ref var p = ref Prefixes[h];

        while (!IsPrefixEmpty(ref p))
        {
            int i;
            for (i = 0; i < NPREF; i++)
            {
                var w = MemToStr(words[i]);
                if (!w.Equals(MemToStr(p.PrefixWords[i])))
                    break;
            }

            if(i == NPREF)
                return h;
            
            h = p.Next;
            p = ref Prefixes[h];
        }

        if(create)
        {
            p = ref Prefixes[h];
            p.PrefixWords = words;
            p.Next = PrefixNext;
            PrefixNext++;
        }
        return h;
    }
    static void AddSuffix(int prefix, Mem suffix)
    {
        ref var s = ref Postfixes[SuffixNext];
        s.SuffixText = suffix;

        ref var p = ref Prefixes[prefix];
       
        int oldFirst = p.FirstPostfix;
        p.FirstPostfix = SuffixNext;
        s.Next = oldFirst;

        SuffixNext++;
    }
    static void Add(Word_Tuple words, Mem suffix)
    {
        var prefix = Lookup(words, true);
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
        while(true) {
            Word_Tuple prefix = default;
            for(var w = 0; w < NPREF; w++)
            {
               var word = GetWord(idx);
               if(word.Start == word.End)
                   return;
                prefix[w] = word;
            }
            var suffix = GetWord(idx);
            if(suffix.Start == suffix.End)
                return;
            Add(prefix, suffix);
        }
    }
}
