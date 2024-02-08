static class Program
{
    static void Main(string[] args)
    {
        #if TEST
        Tests.Run();
        #elif NOALLOC
        var path = Sys.Environment.Arg(0);
        MarkovNoAllocGenerator.Run(path, 10_000);
        #elif ARENA
        var path = Sys.Environment.Arg(0);
        MarkovArenaGenerator.Run(path, 10_000);
        #else
        #error "No configuration specified"
        #endif

    }
}