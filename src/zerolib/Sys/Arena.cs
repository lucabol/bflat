namespace Sys;

using System;
using System.Runtime.CompilerServices;

// Simpler version of https://github.com/lucabol/LNativeMemory/blob/master/LNativeMemory/Arena.cs
// This is an arena allocator that can allocate memory for unmanaged types and spans.
// It is simpler in that it hardens in the zeroing of memory and the lack of bound checking in release.
// In the linked version, that is achieved through a policy system
// as in https://www.lucabol.com/posts/2019-01-29-meta-programming-and-policy-based-design/#implementation
// which doesn't work in bflat as it doesn't support `typeof`. I could investigate.
public unsafe ref struct Arena
{
    private void* _start;
    private void* _nextAlloc;
    private int _size;

    public Arena(int size)
    {
        _start = _nextAlloc = Libc.Malloc(size);
        _size = size;
        Libc.Memset(_start, 0, _size);
    }

    public Arena(Span<byte> memory)
    {
        _start = _nextAlloc = Unsafe.AsPointer<byte>(ref memory[0]);
        _size = memory.Length;
        Libc.Memset(_start, 0, _size);
    }

    public ref T Alloc<T>(int sizeOfType = 0, int alignment = 16) where T : unmanaged
    {
        Debug.Assert(sizeOfType >= 0);
        Debug.Assert(alignment >= 0);
        Debug.Assert(alignment % 2 == 0);

        if (sizeOfType == 0) sizeOfType = sizeof(T);

        _nextAlloc = Align(_nextAlloc, alignment);

        Debug.Assert((ulong)_nextAlloc % (ulong) alignment == 0);
        // It would be simpler with pointer arithmetic, but perhaps there are advantages in doing it this convoluted way.
        Debug.Assert((byte*)_nextAlloc + sizeOfType <= Unsafe.AsPointer(ref Unsafe.Add<byte>(ref Unsafe.AsRef<byte>(_start), (int)_size)),
                "Trying to allocate too many bytes");

        var ptr = _nextAlloc;
        _nextAlloc = (byte*)_nextAlloc + sizeOfType;
        ref var ret = ref Unsafe.AsRef<T>(ptr);

        return ref ret;
    }

    public Span<T> AllocSpan<T>(int n, int sizeOfType = 0, int alignment = 16) where T : unmanaged
    {
        Debug.Assert(sizeOfType >= 0);
        Debug.Assert(alignment >= 0);
        Debug.Assert(alignment % 2 == 0);

        if (sizeOfType == 0) sizeOfType = sizeof(T);
        var sizeOfArray = sizeOfType * n;

        _nextAlloc = Align(_nextAlloc, alignment);

        Debug.Assert((ulong)_nextAlloc % (ulong)alignment == 0);
        Debug.Assert((byte*)_nextAlloc + sizeOfArray <= Unsafe.AsPointer(ref Unsafe.Add<byte>(ref Unsafe.AsRef<byte>(_start), (int)_size)),
                "Trying to allocate too many bytes");

        var ptr = _nextAlloc;
        _nextAlloc = (byte*)_nextAlloc + sizeOfArray;

        return new Span<T>(ptr,n);
    }

    public void Reset()
    {
        _nextAlloc = _start;
        Libc.Memset(_start, 0, _size);
    }

    public long BytesLeft => _size - (long)((byte*)_nextAlloc - (byte*)_start);
    public long TotalBytes => _size;

    public nint  ToPtr<T>(ref T t) => (nint)Unsafe.AsPointer(ref t);
    public ref T ToRef<T>(nint ptr) => ref Unsafe.AsRef<T>((void*)ptr);

    private static void* Align(void* mem, int alignment) => (void*)(((ulong)mem + (ulong)alignment - 1) & ~((ulong)alignment - 1));
}
