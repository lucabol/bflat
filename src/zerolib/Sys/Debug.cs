namespace Sys;

static partial class Debug {
    public static void Assert(bool condition, string message = "")
    {
        #if DEBUG
        if (!condition)
            Environment.Fail(message);
        #endif
    }
}   