static class Program
{
    static void Main(string[] args)
    {
        #if TEST
        Tests.Run();
        #else
        //MarkovGenerator.Run("kjbible.txt"u8, 1000);
        var path = Sys.Environment.Arg(0);
        MarkovNoAllocGenerator.Run(path, 10_000);
        #endif
    }
}