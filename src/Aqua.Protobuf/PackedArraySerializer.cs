// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf;

using System.Collections.Generic;
using System.Collections.ObjectModel;

/// <summary>
/// Provides packing and unpacking of 1-D arrays of fixed-width blittable primitive types
/// as contiguous little-endian byte payloads.
/// </summary>
/// <remarks>
/// <para>
/// Eligible element types are fixed-width blittable scalars: <see cref="sbyte"/>, <see cref="byte"/>,
/// <see cref="short"/>, <see cref="ushort"/>, <see cref="int"/>, <see cref="uint"/>,
/// <see cref="long"/>, <see cref="ulong"/>, <see cref="float"/>, <see cref="double"/>,
/// <see cref="bool"/> (1 byte, 0/1), <see cref="char"/> (UTF-16LE code unit),
/// and version-gated types <c>Half</c> (net5+) and <c>Int128</c>/<c>UInt128</c> (net7+).
/// </para><para>
/// Excluded: <see cref="string"/>, <see cref="decimal"/> (no standard cross-language encoding),
/// <see cref="System.Numerics.BigInteger"/> (variable-length), and date/time types (encoding ambiguity).
/// </para><para>
/// Wire contract: little-endian; element width is fixed per type (see <see cref="TryGetElementWidth"/>).
/// </para>
/// </remarks>
internal static class PackedArraySerializer
{
    private static readonly IReadOnlyDictionary<Type, int> _elementWidths = new ReadOnlyDictionary<Type, int>(new Dictionary<Type, int>
        {
            [typeof(sbyte)] = sizeof(sbyte),
            [typeof(byte)] = sizeof(byte),
            [typeof(short)] = sizeof(short),
            [typeof(ushort)] = sizeof(ushort),
            [typeof(int)] = sizeof(int),
            [typeof(uint)] = sizeof(uint),
            [typeof(long)] = sizeof(long),
            [typeof(ulong)] = sizeof(ulong),
            [typeof(float)] = sizeof(float),
            [typeof(double)] = sizeof(double),
            [typeof(bool)] = sizeof(bool),
            [typeof(char)] = sizeof(char),
#if NET5_0_OR_GREATER
            [typeof(Half)] = 2,
#endif // NET5_0_OR_GREATER
#if NET7_0_OR_GREATER
            [typeof(Int128)] = 16,
            [typeof(UInt128)] = 16,
#endif // NET7_0_OR_GREATER
        });

    /// <summary>
    /// Gets the complete set of element types eligible for packed-array serialization.
    /// </summary>
    public static IEnumerable<Type> EligibleElementTypes => _elementWidths.Keys;

    /// <summary>
    /// Returns <see langword="true"/> if <paramref name="elementType"/> is eligible for packed-array serialization.
    /// </summary>
    public static bool IsEligibleElementType(Type elementType)
        => elementType is not null && _elementWidths.ContainsKey(elementType);

    /// <summary>
    /// Returns <see langword="true"/> and the fixed byte-width for <paramref name="elementType"/> if eligible.
    /// </summary>
    public static bool TryGetElementWidth(Type elementType, out int width)
    {
        if (elementType is not null && _elementWidths.TryGetValue(elementType, out width))
        {
            return true;
        }

        width = 0;
        return false;
    }

    /// <summary>
    /// Packs <paramref name="array"/> into a little-endian byte payload.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="array"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the element type of <paramref name="array"/> is not eligible.</exception>
    public static byte[] Pack(Array array)
    {
        array.AssertNotNull();

        var elementType = array.GetType().GetElementType()!;
        if (elementType.IsEnum)
        {
            elementType = Enum.GetUnderlyingType(elementType);
        }

        if (!_elementWidths.TryGetValue(elementType, out var width))
        {
            throw new ArgumentException($"Element type '{elementType}' is not eligible for packed-array serialization.", nameof(array));
        }

        if (array.Length == 0)
        {
            return [];
        }

        return PackByElement(array, elementType, width);
    }

    /// <summary>
    /// Unpacks a little-endian byte payload back to a typed <see cref="Array"/> with element type <paramref name="elementType"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="bytes"/> or <paramref name="elementType"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="elementType"/> is not eligible or payload length is not aligned to element width.</exception>
    public static Array Unpack(byte[] bytes, Type elementType)
    {
        bytes.AssertNotNull();
        elementType.AssertNotNull();

        if (!_elementWidths.TryGetValue(elementType, out var width))
        {
            throw new ArgumentException($"Element type '{elementType}' is not eligible for packed-array serialization.", nameof(elementType));
        }

        if (width > 1 && bytes.Length % width != 0)
        {
            throw new ArgumentException($"Byte payload length {bytes.Length} is not a multiple of element width {width} for type '{elementType}'.", nameof(bytes));
        }

        return UnpackByElement(bytes, elementType, width);
    }

    private static byte[] PackByElement(Array array, Type elementType, int width)
    {
        var result = new byte[array.Length * width];
        var offset = 0;

        if (elementType == typeof(sbyte))
        {
            foreach (sbyte v in (sbyte[])array)
            {
                result[offset++] = (byte)v;
            }
        }
        else if (elementType == typeof(byte))
        {
            Buffer.BlockCopy(array, 0, result, 0, result.Length);
        }
        else if (elementType == typeof(short))
        {
            foreach (short v in (short[])array)
            {
                WriteLE2(result, offset, (ushort)v);
                offset += 2;
            }
        }
        else if (elementType == typeof(ushort))
        {
            foreach (ushort v in (ushort[])array)
            {
                WriteLE2(result, offset, v);
                offset += 2;
            }
        }
        else if (elementType == typeof(int))
        {
            foreach (int v in (int[])array)
            {
                WriteLE4(result, offset, (uint)v);
                offset += 4;
            }
        }
        else if (elementType == typeof(uint))
        {
            foreach (uint v in (uint[])array)
            {
                WriteLE4(result, offset, v);
                offset += 4;
            }
        }
        else if (elementType == typeof(long))
        {
            foreach (long v in (long[])array)
            {
                WriteLE8(result, offset, (ulong)v);
                offset += 8;
            }
        }
        else if (elementType == typeof(ulong))
        {
            foreach (ulong v in (ulong[])array)
            {
                WriteLE8(result, offset, v);
                offset += 8;
            }
        }
        else if (elementType == typeof(float))
        {
            foreach (float v in (float[])array)
            {
                WriteLE4(result, offset, FloatToUInt32(v));
                offset += 4;
            }
        }
        else if (elementType == typeof(double))
        {
            foreach (double v in (double[])array)
            {
                WriteLE8(result, offset, DoubleToUInt64(v));
                offset += 8;
            }
        }
        else if (elementType == typeof(bool))
        {
            foreach (bool v in (bool[])array)
            {
                result[offset++] = v ? (byte)1 : (byte)0;
            }
        }
        else if (elementType == typeof(char))
        {
            foreach (char v in (char[])array)
            {
                WriteLE2(result, offset, v);
                offset += 2;
            }
        }
#if NET5_0_OR_GREATER
        else if (elementType == typeof(Half))
        {
            foreach (Half v in (Half[])array)
            {
                WriteLE2(result, offset, (ushort)BitConverter.HalfToInt16Bits(v));
                offset += 2;
            }
        }
#endif // NET5_0_OR_GREATER
#if NET7_0_OR_GREATER
        else if (elementType == typeof(Int128))
        {
            foreach (Int128 v in (Int128[])array)
            {
                WriteLE16(result, offset, (UInt128)v);
                offset += 16;
            }
        }
        else if (elementType == typeof(UInt128))
        {
            foreach (UInt128 v in (UInt128[])array)
            {
                WriteLE16(result, offset, v);
                offset += 16;
            }
        }
#endif // NET7_0_OR_GREATER

        return result;
    }

    private static Array UnpackByElement(byte[] bytes, Type elementType, int width)
    {
        var count = width == 1 ? bytes.Length : bytes.Length / width;

        if (count == 0)
        {
            return Array.CreateInstance(elementType, 0);
        }

        if (elementType == typeof(sbyte))
        {
            var a = new sbyte[count];
            for (var i = 0; i < count; i++)
            {
                a[i] = (sbyte)bytes[i];
            }

            return a;
        }

        if (elementType == typeof(byte))
        {
            var a = new byte[count];
            Buffer.BlockCopy(bytes, 0, a, 0, count);
            return a;
        }

        if (elementType == typeof(short))
        {
            var a = new short[count];
            for (var i = 0; i < count; i++)
            {
                a[i] = (short)ReadLE2(bytes, i * 2);
            }

            return a;
        }

        if (elementType == typeof(ushort))
        {
            var a = new ushort[count];
            for (var i = 0; i < count; i++)
            {
                a[i] = ReadLE2(bytes, i * 2);
            }

            return a;
        }

        if (elementType == typeof(int))
        {
            var a = new int[count];
            for (var i = 0; i < count; i++)
            {
                a[i] = (int)ReadLE4(bytes, i * 4);
            }

            return a;
        }

        if (elementType == typeof(uint))
        {
            var a = new uint[count];
            for (var i = 0; i < count; i++)
            {
                a[i] = ReadLE4(bytes, i * 4);
            }

            return a;
        }

        if (elementType == typeof(long))
        {
            var a = new long[count];
            for (var i = 0; i < count; i++)
            {
                a[i] = (long)ReadLE8(bytes, i * 8);
            }

            return a;
        }

        if (elementType == typeof(ulong))
        {
            var a = new ulong[count];
            for (var i = 0; i < count; i++)
            {
                a[i] = ReadLE8(bytes, i * 8);
            }

            return a;
        }

        if (elementType == typeof(float))
        {
            var a = new float[count];
            for (var i = 0; i < count; i++)
            {
                a[i] = UInt32ToFloat(ReadLE4(bytes, i * 4));
            }

            return a;
        }

        if (elementType == typeof(double))
        {
            var a = new double[count];
            for (var i = 0; i < count; i++)
            {
                a[i] = UInt64ToDouble(ReadLE8(bytes, i * 8));
            }

            return a;
        }

        if (elementType == typeof(bool))
        {
            var a = new bool[count];
            for (var i = 0; i < count; i++)
            {
                a[i] = bytes[i] != 0;
            }

            return a;
        }

        if (elementType == typeof(char))
        {
            var a = new char[count];
            for (var i = 0; i < count; i++)
            {
                a[i] = (char)ReadLE2(bytes, i * 2);
            }

            return a;
        }

#if NET5_0_OR_GREATER
        if (elementType == typeof(Half))
        {
            var a = new Half[count];
            for (var i = 0; i < count; i++)
            {
                a[i] = BitConverter.Int16BitsToHalf((short)ReadLE2(bytes, i * 2));
            }

            return a;
        }
#endif // NET5_0_OR_GREATER
#if NET7_0_OR_GREATER
        if (elementType == typeof(Int128))
        {
            var a = new Int128[count];
            for (var i = 0; i < count; i++)
            {
                a[i] = (Int128)ReadLE16(bytes, i * 16);
            }

            return a;
        }

        if (elementType == typeof(UInt128))
        {
            var a = new UInt128[count];
            for (var i = 0; i < count; i++)
            {
                a[i] = ReadLE16(bytes, i * 16);
            }

            return a;
        }
#endif // NET7_0_OR_GREATER

        throw new ArgumentException($"Element type '{elementType}' is not supported.", nameof(elementType));
    }

    private static void WriteLE2(byte[] buf, int offset, ushort v)
    {
        buf[offset] = (byte)(v & 0xFF);
        buf[offset + 1] = (byte)(v >> 8);
    }

    private static void WriteLE4(byte[] buf, int offset, uint v)
    {
        buf[offset] = (byte)(v & 0xFF);
        buf[offset + 1] = (byte)((v >> 8) & 0xFF);
        buf[offset + 2] = (byte)((v >> 16) & 0xFF);
        buf[offset + 3] = (byte)(v >> 24);
    }

    private static void WriteLE8(byte[] buf, int offset, ulong v)
    {
        buf[offset] = (byte)(v & 0xFF);
        buf[offset + 1] = (byte)((v >> 8) & 0xFF);
        buf[offset + 2] = (byte)((v >> 16) & 0xFF);
        buf[offset + 3] = (byte)((v >> 24) & 0xFF);
        buf[offset + 4] = (byte)((v >> 32) & 0xFF);
        buf[offset + 5] = (byte)((v >> 40) & 0xFF);
        buf[offset + 6] = (byte)((v >> 48) & 0xFF);
        buf[offset + 7] = (byte)(v >> 56);
    }

    private static ushort ReadLE2(byte[] buf, int offset)
        => (ushort)(buf[offset] | (buf[offset + 1] << 8));

    private static uint ReadLE4(byte[] buf, int offset)
        => (uint)(buf[offset] | (buf[offset + 1] << 8) | (buf[offset + 2] << 16) | (buf[offset + 3] << 24));

    private static ulong ReadLE8(byte[] buf, int offset)
        => (ulong)buf[offset]
        | ((ulong)buf[offset + 1] << 8)
        | ((ulong)buf[offset + 2] << 16)
        | ((ulong)buf[offset + 3] << 24)
        | ((ulong)buf[offset + 4] << 32)
        | ((ulong)buf[offset + 5] << 40)
        | ((ulong)buf[offset + 6] << 48)
        | ((ulong)buf[offset + 7] << 56);

#if NET5_0_OR_GREATER
    private static uint FloatToUInt32(float v) => BitConverter.SingleToUInt32Bits(v);

    private static float UInt32ToFloat(uint v) => BitConverter.UInt32BitsToSingle(v);

    private static ulong DoubleToUInt64(double v) => BitConverter.DoubleToUInt64Bits(v);

    private static double UInt64ToDouble(ulong v) => BitConverter.UInt64BitsToDouble(v);
#else
    private static uint FloatToUInt32(float v)
    {
        var b = BitConverter.GetBytes(v);
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(b);
        }

        return ReadLE4(b, 0);
    }

    private static float UInt32ToFloat(uint v)
    {
        var b = new byte[4];
        WriteLE4(b, 0, v);
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(b);
        }

        return BitConverter.ToSingle(b, 0);
    }

    private static ulong DoubleToUInt64(double v)
    {
        var b = BitConverter.GetBytes(v);
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(b);
        }

        return ReadLE8(b, 0);
    }

    private static double UInt64ToDouble(ulong v)
    {
        var b = new byte[8];
        WriteLE8(b, 0, v);
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(b);
        }

        return BitConverter.ToDouble(b, 0);
    }
#endif // NET5_0_OR_GREATER

#if NET7_0_OR_GREATER
    private static void WriteLE16(byte[] buf, int offset, UInt128 v)
    {
        WriteLE8(buf, offset, (ulong)(v & ulong.MaxValue));
        WriteLE8(buf, offset + 8, (ulong)(v >> 64));
    }

    private static UInt128 ReadLE16(byte[] buf, int offset)
        => (UInt128)ReadLE8(buf, offset) | ((UInt128)ReadLE8(buf, offset + 8) << 64);
#endif // NET7_0_OR_GREATER
}
