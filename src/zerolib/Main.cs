static class Program
{
    static void Main(string[] args)
    {
        Tests.Run();
        MarkovGenerator.Run("psalms.txt"u8, 100);
    }
}