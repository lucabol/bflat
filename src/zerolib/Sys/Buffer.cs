namespace Sys;

// In a no-allocation world, it is nice to have an easy way to create buffers of specific sizes (i.e., for command line args, file text, etc...).
// TODO: it is terrible not to able to get the size of an inline array from the instance, otherwise I need to pass the size around.
public static partial class Buffers
{
    const int K     = 1024;
    const int K8    = K * 8;

    [System.Runtime.CompilerServices.InlineArray(K)]
    public struct K_Buffer<T> { private T _element0; }

    [System.Runtime.CompilerServices.InlineArray(K8)]
    public struct K8_Buffer<T> { private T _element0; }
}
