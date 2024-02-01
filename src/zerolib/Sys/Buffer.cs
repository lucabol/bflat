using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sys;

// In a no-allocation world, it is nice to have an easy way to create buffers of specific sizes (i.e., for command line args, file text, etc...).
// TODO: it is terrible not to able to get the size of an inline array from the instance, otherwise I need to pass the size around.
public static partial class Buffers
{
    public const int K     = 1024;
    public const int K4    = K * 4;
    public const int K8    = K * 8;
    public const int K16   = K * 16;
    public const int K32   = K * 32;

    public const int M     = K * K;
    public const int M8    = M * 8;

    [InlineArray(K)]
    public struct K_Buffer<T> { private T _element0; }

    [InlineArray(K4)]
    public struct K4_Buffer<T> { private T _element0; }

    [InlineArray(K8)]
    public struct K8_Buffer<T> { private T _element0; }

    [InlineArray(K16)]
    public struct K16_Buffer<T> { private T _element0; }

    [InlineArray(K32)]
    public struct K32_Buffer<T> { private T _element0; }

    // As it happens, inline arrays cannot be bigger than 1 Megabyte. No sure why.
    public unsafe struct M8_Buffer {
        fixed byte _buf[M8];

        public System.Span<byte> Span {
        get {
            fixed (byte* p = _buf)
                return new System.Span<byte>(p, M8);
        } }
    }

    public unsafe struct HugeBuffer<T> where T: unmanaged
    {
        fixed byte _buf[M8];

        //public ref T this[int index] => ref Unsafe.As<byte, T>(ref _buf[index * sizeof(T)]);

        public ref T this[int index]
        {
            get {
                if(index > Length)
                    Sys.Environment.Fail("Index out of range"u8);
                return ref Unsafe.Add(ref Unsafe.As<byte, T>(ref _buf[0]), index);
            }
        }
        public static implicit operator System.Span<T>(HugeBuffer<T> buffer)
            => MemoryMarshal.CreateSpan(ref Unsafe.As<byte,T>(ref buffer._buf[0]), M8 / sizeof(T)); 
        public int Length => M8 / sizeof(T);
    }
    /*
    // Mads please implement this
    public unsafe struct M8_Buffer<T> where T: unmanaged {
        public fixed T _buf[M8];

        public System.Span<T> Span {
        get {
            fixed (T* p = _buf)
                return new System.Span<T>(p, M8);
        } }
    }
    */
}
