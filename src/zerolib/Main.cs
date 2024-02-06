static class Program
{
    static void Main(string[] args)
    {
        #if TEST
        Tests.Run();
        #endif
        //MarkovGenerator.Run("kjbible.txt"u8, 1000);
        var path = Sys.Environment.Arg(0);
        MarkovGenerator.Run(path, 10_000);
    }
}