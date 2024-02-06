namespace Sys;

public static class Assert
{
    public static void True(bool condition)
    {
        if (!condition)
            Environment.Fail("Assertion failed"u8);
    }
    public static void False(bool condition)
    {
        if (condition)
            Environment.Fail("Assertion failed for ints."u8);
    }
    public static void Equal(int a, int b)
    {
        if (a != b)
            Environment.Fail("Assertion failed"u8);
    }
    public static void Equal(double a, double b)
    {
        if (a != b)
            Environment.Fail("Assertion failed for doubles."u8);
    }
}