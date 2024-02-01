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
    const int NPREFIXES = Buffers.K16;
    const int NSUFFIXES = Buffers.K16;

    static Buffers.K16_Buffer<int>     Hashes;
    static Buffers.K16_Buffer<Prefix>  Prefixes;
    static Buffers.K16_Buffer<Suffix>  Suffixes;
    static Buffers.M8_Buffer           Text;
    static int TextLength;

    static int PrefixNext = 1; // We use 0 as a sentinel value for empty prefixes.
    static int SuffixNext;

    public static void Run(Str8 path, int nwords)
    {
        var txt = File.Slurp(path, Text.Span);
        TextLength = txt.Length;


        Build();
        Debug();
    }

    static void Debug()
    {
        for(var i = 0; i < NPREFIXES; i++)
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
                idx = word.End;
            }
            var suffix = GetWord(idx);
            if(suffix.Start == suffix.End)
                return;
            Add(prefix, suffix);
            idx = suffix.End;
        }
    }
}
