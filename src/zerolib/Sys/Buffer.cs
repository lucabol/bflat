using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sys;

// In a no-allocation world, it is nice to have an easy way to create buffers of specific sizes (i.e., for command line args, file text, etc...).
// TODO: it is terrible not to able to get the size of an inline array from the instance, otherwise I need to pass the size around.
public static partial class Buffers
{
    public const int K     = 1024;
    public const int K8    = K * 8;
    public const int K16   = K * 16;
    public const int K300  = K * 300;

    public const int M      = K * K;
    public const int M8     = M * 8;
    public const int M64    = M * 64;

    // Inline arrays cannot be bigger than 1Mb (or sometimes a combination of smaller ones don't compile either, ?!).
    [InlineArray(K)]
    public struct K_Buffer<T> { private T _element0; }

    [InlineArray(K8)]
    public struct K8_Buffer<T> { private T _element0; }

    [InlineArray(K16)]
    public struct K16_Buffer<T> { private T _element0; }

    public unsafe struct HugeBuffer<T> where T: unmanaged
    {
        fixed byte _buf[M64];

        //public ref T this[int index] => ref Unsafe.As<byte, T>(ref _buf[index * sizeof(T)]);

        public ref T this[int index]
        {
            get {
                if(index < 0 || index >= Length)
                    Sys.Environment.Fail("Index out of range"u8);
                return ref Unsafe.Add(ref Unsafe.As<byte, T>(ref _buf[0]), index);
            }
        }
        public static implicit operator System.Span<T>(HugeBuffer<T> buffer)
            => MemoryMarshal.CreateSpan(ref Unsafe.As<byte,T>(ref buffer._buf[0]), M64 / sizeof(T)); 
        public int Length => M64 / sizeof(T);
    }


    public const int BIGSIZE = 300_000;

    public unsafe struct BigIntBuffer
    {
        fixed byte _buf[BIGSIZE * sizeof(int)];

        public ref int this[int index]
        {
            get {
                if(index < 0 || index >= BIGSIZE)
                    Sys.Environment.Fail("Index out of range"u8);
                return ref Unsafe.Add(ref Unsafe.As<byte, int>(ref _buf[0]), index);
            }
        }
        public static implicit operator System.Span<int>(BigIntBuffer buffer)
            => MemoryMarshal.CreateSpan(ref Unsafe.As<byte,int>(ref buffer._buf[0]), BIGSIZE); 

        public int Length => BIGSIZE;
    }

    // Using HugeBuffer<byte> gives compiler errors, so I need to create a new type.
    public unsafe struct M8_Buffer {
        fixed byte _buf[M8];

        public System.Span<byte> Span {
        get {
            fixed (byte* p = _buf)
                return new System.Span<byte>(p, M8);
        } }
    }

}
