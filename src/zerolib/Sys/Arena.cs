namespace Sys;

using System;
using System.Runtime.CompilerServices;

public unsafe ref struct Arena
{
    private void* _start;
    private void* _nextAlloc;
    private int _size;

    public Arena(Span<byte> memory)
    {
        _start = _nextAlloc = Unsafe.AsPointer<byte>(ref memory[0]);
        _size = memory.Length;
    }

    public ref T Alloc<T>(int sizeOfType = 0, int alignment = 16) where T : unmanaged
    {
        Debug.Assert(sizeOfType >= 0);
        Debug.Assert(alignment >= 0);
        Debug.Assert(alignment % 2 == 0);

        if (sizeOfType == 0) sizeOfType = sizeof(T);

        _nextAlloc = Align(_nextAlloc, alignment);

        Debug.Assert((ulong)_nextAlloc % (ulong) alignment == 0);
        Debug.Assert((byte*)_nextAlloc + sizeOfType <= Unsafe.Add<byte>(Unsafe.AsRef<byte>(_start), (int)_size),
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
        Debug.Assert((byte*)_nextAlloc + sizeOfArray <= Unsafe.Add<byte>(ref Unsafe.AsRef<byte>(_start), (int)_size),
                "Trying to allocate too many bytes");

        var ptr = _nextAlloc;
        _nextAlloc = (byte*)_nextAlloc + sizeOfArray;

        return new Span<T>(ptr,n);
    }

    public void Reset()
    {
        _nextAlloc = _start;
    }

    public long BytesLeft => _size - (long)((byte*)_nextAlloc - (byte*)_start);
    public long TotalBytes => _size;

    private static void* Align(void* mem, int alignment) => (void*)(((ulong)mem + (ulong)alignment - 1) & ~((ulong)alignment - 1));
}
