// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if NETSTANDARD2_0 || NETFRAMEWORK

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Aqua;
#pragma warning restore IDE0130 // Namespace does not match folder structure

using System.Buffers;

/// <summary>
/// Polyfill type for <c>System.Buffers.ArrayBufferWriter&lt;T&gt;</c>.
/// </summary>
internal sealed class ArrayBufferWriter<T>(int initialSize = 256) : IBufferWriter<T>
{
    private T[] _buffer = new T[initialSize];
    private int _index;

    public void Advance(int count) => _index += count;

    public Memory<T> GetMemory(int sizeHint = 0)
    {
        Ensure(sizeHint);
        return _buffer.AsMemory(_index);
    }

    public Span<T> GetSpan(int sizeHint = 0)
    {
        Ensure(sizeHint);
        return _buffer.AsSpan(_index);
    }

    public ReadOnlySpan<T> WrittenSpan => _buffer.AsSpan(0, _index);

    private void Ensure(int sizeHint)
    {
        if (_index + sizeHint <= _buffer.Length)
        {
            return;
        }

        var newSize = Math.Max(_buffer.Length * 2, _index + sizeHint);
        Array.Resize(ref _buffer, newSize);
    }
}

#endif // NETSTANDARD2_0 || NETFRAMEWORK
