#if STANDARD
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public static class MarkovStandardGenerator
{
    const int NWORDS = 2;

    public static void Run(string path, int nwords)
    {
        var text = File.ReadAllText(path);
        var words = text.Split(' ');

        var hash = Build(words);
        Generate(hash, nwords);
    }

    class StringArrayEqualityComparer : EqualityComparer<string[]>
    {
        public override bool Equals(string[]? x, string[]? y) {
            return System.Collections.StructuralComparisons.StructuralEqualityComparer.Equals(x, y);
        }
        public override int GetHashCode(string[] obj) {
            return System.Collections.StructuralComparisons.StructuralEqualityComparer.GetHashCode(obj);
        }
    }

    static Dictionary<string[], List<string>> Build(string[] words)
    {
        var hash = new Dictionary<string[], List<string>>(new StringArrayEqualityComparer());

        // This is the empiric size of the hash table that ends up being used for the kjbible.
        // Preallocating it so that it doesn't have to resize, as the other implementations don't.
        hash.EnsureCapacity(300_000);

        for (int i = 0; i < words.Length - NWORDS; i++)
        {
            var key = new string[NWORDS];
            for(int j = 0; j < NWORDS; j++)
            {
                key[j] = words[i + j];
            }
            if (!hash.ContainsKey(key))
            {
                hash[key] = new List<string>();
            }
            hash[key].Add(words[i + NWORDS]);
        }
        return hash;
    }
    static void Generate(Dictionary<string[], List<string>> hash, int nwords)
    {
        var rand  = new Random();
        var words = hash.Keys.ElementAt(rand.Next(hash.Count));

        for (int i = 0; i < nwords; i++)
        {
            Console.Write($"{words[0]} ");
            // My intuition is that the key is not in the has just for
            // the last word, hence it is inefficient, but simpler to check every time.
            if(hash.TryGetValue(words, out var next))
            {
                words = words.Skip(1).Append(next[rand.Next(next.Count)]).ToArray();
            } else {
                hash.Keys.ElementAt(rand.Next(hash.Count));
            }
        }
    }
}

#endif
