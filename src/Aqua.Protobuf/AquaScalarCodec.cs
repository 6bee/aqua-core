// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf;

using System.Buffers;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

/// <summary>
/// Deterministic byte encoding for the scalar leaf types.
/// </summary>
internal static class AquaScalarCodec
{
    private const byte MicrosecondsTag = 1;
    private const byte NanosecondsTag = 2;

    public static void Encode(IBufferWriter<byte> writer, object value, ProtoOptions options)
    {
        switch (value)
        {
            case bool v: writer.Write(v); return;
            case byte v: writer.Write(v); return;
            case sbyte v: writer.Write(v); return;
            case char v: writer.Write(v); return;
            case short v: writer.Write(v); return;
            case ushort v: writer.Write(v); return;
            case int v: writer.Write(v); return;
            case uint v: writer.Write(v); return;
            case long v: writer.Write(v); return;
            case ulong v: writer.Write(v); return;
            case float v: writer.Write(v); return;
            case double v: writer.Write(v); return;
            case decimal v: writer.Write(v); return;
            case Guid v: writer.Write(v); return;
            case DateTime v: writer.Write(v, options); return;
            case DateTimeOffset v: writer.Write(v, options); return;
            case TimeSpan v: writer.Write(v, options); return;
            case BigInteger v: writer.Write(v); return;
            case Complex v: writer.Write(v); return;
#if NET5_0_OR_GREATER
            case Half v: writer.Write(v); return;
#endif // NET5_0_OR_GREATER
#if NET6_0_OR_GREATER
            case DateOnly v: writer.Write(v); return;
            case TimeOnly v: writer.Write(v); return;
#endif // NET6_0_OR_GREATER
#if NET7_0_OR_GREATER
            case Int128 v: writer.Write(v); return;
            case UInt128 v: writer.Write(v); return;
#endif // NET7_0_OR_GREATER
        }

        var type = value.GetType();
        throw new InvalidOperationException($"Scalar type '{type}' is not supported by the Aqua protobuf scalar codec.");
    }

    public static object Decode(scoped ReadOnlySpan<byte> data, Type type, ProtoOptions options)
    {
        if (type == typeof(bool))
        {
            return data.ReadBoolean();
        }

        if (type == typeof(byte))
        {
            return data.ReadByte();
        }

        if (type == typeof(sbyte))
        {
            return data.ReadSByte();
        }

        if (type == typeof(char))
        {
            return data.ReadChar();
        }

        if (type == typeof(short))
        {
            return data.ReadInt16();
        }

        if (type == typeof(ushort))
        {
            return data.ReadUInt16();
        }

        if (type == typeof(int))
        {
            return data.ReadInt32();
        }

        if (type == typeof(uint))
        {
            return data.ReadUInt32();
        }

        if (type == typeof(long))
        {
            return data.ReadInt64();
        }

        if (type == typeof(ulong))
        {
            return data.ReadUInt64();
        }

        if (type == typeof(float))
        {
            return data.ReadSingle();
        }

        if (type == typeof(double))
        {
            return data.ReadDouble();
        }

        if (type == typeof(decimal))
        {
            return data.ReadDecimal();
        }

        if (type == typeof(Guid))
        {
            return data.ReadGuid();
        }

        if (type == typeof(DateTime))
        {
            return data.ReadDateTime(options);
        }

        if (type == typeof(DateTimeOffset))
        {
            return data.ReadDateTimeOffset(options);
        }

        if (type == typeof(TimeSpan))
        {
            return data.ReadTimeSpan(options);
        }

        if (type == typeof(BigInteger))
        {
            return data.ReadBigInteger();
        }

        if (type == typeof(Complex))
        {
            return data.ReadComplex();
        }

#if NET5_0_OR_GREATER
        if (type == typeof(Half))
        {
            return data.ReadHalf();
        }
#endif // NET5_0_OR_GREATER

#if NET6_0_OR_GREATER
        if (type == typeof(DateOnly))
        {
            return data.ReadDateOnly();
        }

        if (type == typeof(TimeOnly))
        {
            return data.ReadTimeOnly();
        }
#endif // NET6_0_OR_GREATER

#if NET7_0_OR_GREATER
        if (type == typeof(Int128))
        {
            return data.ReadInt128();
        }

        if (type == typeof(UInt128))
        {
            return data.ReadUInt128();
        }
#endif // NET7_0_OR_GREATER

        throw new InvalidOperationException($"Scalar type '{type}' is not supported by the Aqua protobuf scalar codec.");
    }

    extension(IBufferWriter<byte> writer)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(byte value)
        {
            Span<byte> buffer = writer.GetSpan(1);
            buffer[0] = value;
            writer.Advance(1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(bool value) => writer.Write((byte)(value ? 1 : 0));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(sbyte value) => writer.Write((byte)value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(char value)
        {
            const string ErrorMessage = "A surrogate code unit cannot be written as a single character.";
#if NETSTANDARD
            if (char.IsSurrogate(value))
            {
                throw new ArgumentException(ErrorMessage, nameof(value));
            }

            byte[] bytes = new byte[3];
            int byteCount = Encoding.UTF8.GetBytes([value], 0, 1, bytes, 0);
            writer.Write(bytes.AsSpan(0, byteCount));
#else
            if (!Rune.TryCreate(value, out var rune))
            {
                throw new ArgumentException(ErrorMessage, nameof(value));
            }

            Span<byte> buffer = stackalloc byte[4];
            int utf8ByteCount = rune.EncodeToUtf8(buffer);
            writer.Write(buffer[..utf8ByteCount]);
#endif // NETSTANDARD
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(short value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(short)];
            BinaryPrimitives.WriteInt16LittleEndian(buffer, value);
            writer.Write(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(ushort value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(ushort)];
            BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
            writer.Write(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(int value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(int)];
            BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
            writer.Write(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(uint value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(uint)];
            BinaryPrimitives.WriteUInt32LittleEndian(buffer, value);
            writer.Write(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(long value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(long)];
            BinaryPrimitives.WriteInt64LittleEndian(buffer, value);
            writer.Write(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(ulong value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(ulong)];
            BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);
            writer.Write(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(float value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(int)];
            BinaryPrimitives.WriteInt32LittleEndian(
                buffer,
                BitConverter.SingleToInt32Bits(value));
            writer.Write(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(double value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(long)];
            BinaryPrimitives.WriteInt64LittleEndian(
                buffer,
                BitConverter.DoubleToInt64Bits(value));
            writer.Write(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(Complex value)
        {
            writer.Write(value.Real);
            writer.Write(value.Imaginary);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(decimal value)
        {
            // Offset  Size   Meaning
            // ------  ----   --------------------------
            // 0       12     96-bit integer (little-endian)
            // 12      1      scale (0–28)
            // 13      1      sign (0 = +, 1 = -)
            // 14–15   2      reserved / padding (0)

            // 12B LE magnitude | 1B scale | 1B sign | 2B reserved
#if NETSTANDARD2_0
            var bits = decimal.GetBits(value);
#else
            Span<int> bits = stackalloc int[4];
            _ = decimal.GetBits(value, bits);
#endif // NETSTANDARD2_0

            int scale = (bits[3] >> 16) & 0xFF;
            byte sign = (bits[3] & unchecked((int)0x80000000)) != 0 ? (byte)1 : (byte)0;

            Span<byte> buffer = stackalloc byte[16];

            // write 96-bit magnitude (little-endian, exactly like .NET layout)
            BinaryPrimitives.WriteInt32LittleEndian(buffer[..4], bits[0]);
            BinaryPrimitives.WriteInt32LittleEndian(buffer[4..8], bits[1]);
            BinaryPrimitives.WriteInt32LittleEndian(buffer[8..12], bits[2]);

            buffer[12] = (byte)scale;
            buffer[13] = sign;

            buffer[14] = 0;
            buffer[15] = 0;

            writer.Write(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(BigInteger value)
        {
            var bytes = value.ToByteArray();
            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        private void Write(TimeSpan value, ProtoOptions options)
        {
            switch (options.TimeSpanEncoding)
            {
                case TimeSpanEncoding.Auto:
                    {
                        var ticks = value.Ticks;
                        if (TimeSpanHelper.FitsInNanoseconds(ticks) &&
                            ticks % 10 != 0)
                        {
                            writer.Write(NanosecondsTag);
                            writer.Write(TimeSpanHelper.TicksToNanoseconds(ticks)); // nanoseconds
                        }
                        else
                        {
                            writer.Write(MicrosecondsTag);
                            writer.Write(TimeSpanHelper.TicksToMioseconds(ticks)); // microseconds
                        }

                        break;
                    }

                case TimeSpanEncoding.Microseconds:
                    writer.Write(TimeSpanHelper.TicksToMioseconds(value.Ticks)); // microseconds
                    break;

                case TimeSpanEncoding.Nanoseconds:
                    writer.Write(TimeSpanHelper.TicksToNanoseconds(value.Ticks)); // nanoseconds
                    break;

                default:
                    throw new ProtobufSerializationException($"Timespan encoding option {options.TimeSpanEncoding} is not supported");
            }
        }

        private void Write(DateTime value, ProtoOptions options)
        {
            if (value.Kind is not DateTimeKind.Utc)
            {
                value = value.ToUniversalTime();
            }

            switch (options.DateTimeEncoding)
            {
                case DateTimeEncoding.Auto:
                    {
                        // Encode as Unix nanoseconds only if required to preserve sub-microsecond precision.
                        // DateTime ticks are 100ns units; 10 ticks = 1 μs.
                        var ticks = value.Ticks;
                        if (ticks >= DateTime.UnixNanosecondsMinTicks &&
                            ticks <= DateTime.UnixNanosecondsMaxTicks &&
                            ticks % 10 != 0)
                        {
                            writer.Write(NanosecondsTag); // Unix nanoseconds
                            writer.Write(value.ToUnixNanoseconds());
                        }
                        else
                        {
                            writer.Write(MicrosecondsTag); // Unix microseconds
                            writer.Write(value.ToUnixMicroseconds());
                        }

                        break;
                    }

                case DateTimeEncoding.UnixMicroseconds:
                    writer.Write(value.ToUnixMicroseconds());
                    break;

                case DateTimeEncoding.UnixNanoseconds:
                    writer.Write(value.ToUnixNanoseconds());
                    break;

                default:
                    throw new ProtobufSerializationException($"Datetime encoding option {options.DateTimeEncoding} is not supported");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(DateTimeOffset value, ProtoOptions options)
        {
            // We're writing a *local* DateTime value in msgpack encoding as if it were UTC time
            writer.Write(new DateTime(value.Ticks, DateTimeKind.Utc), options);
            writer.Write((short)value.Offset.TotalMinutes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(Guid value)
        {
#if NETSTANDARD
            var bytes = value.ToByteArray();

            // Convert .NET Guid layout to RFC 4122 layout.
            Array.Reverse(bytes, 0, 4);
            Array.Reverse(bytes, 4, 2);
            Array.Reverse(bytes, 6, 2);

            writer.Write(bytes);
#else
            Span<byte> bytes = stackalloc byte[16];
            _ = value.TryWriteBytes(bytes, true, out _);
            writer.Write(bytes);
#endif // NETSTANDARD
        }

#if NET7_0_OR_GREATER

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(Int128 value)
        {
            writer.Write((long)value);
            writer.Write((long)(value >> 64));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(UInt128 value)
        {
            writer.Write((ulong)value);
            writer.Write((ulong)(value >> 64));
        }
#endif // NET7_0_OR_GREATER

#if NET6_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(DateOnly value)
        {
            writer.Write(value.DayNumber);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(TimeOnly value)
        {
            writer.Write(value.Ticks * 100L); // nanoseconds since midnight
        }
#endif // NET6_0_OR_GREATER

#if NET5_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(Half value)
        {
            writer.Write(BitConverter.HalfToUInt16Bits(value));
        }
#endif // NET5_0_OR_GREATER
    }

    extension(scoped ref ReadOnlySpan<byte> span)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ReadBoolean()
        {
            var value = span[0] != 0;
            span = span[sizeof(bool)..];
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte ReadByte()
        {
            var value = span[0];
            span = span[sizeof(byte)..];
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private sbyte ReadSByte()
        {
            var value = (sbyte)span[0];
            span = span[sizeof(sbyte)..];
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private short ReadInt16()
        {
            var value = BinaryPrimitives.ReadInt16LittleEndian(span);
            span = span[sizeof(short)..];
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ushort ReadUInt16()
        {
            var value = BinaryPrimitives.ReadUInt16LittleEndian(span);
            span = span[sizeof(ushort)..];
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int ReadInt32()
        {
            var value = BinaryPrimitives.ReadInt32LittleEndian(span);
            span = span[sizeof(int)..];
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint ReadUInt32()
        {
            var value = BinaryPrimitives.ReadUInt32LittleEndian(span);
            span = span[sizeof(uint)..];
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private long ReadInt64()
        {
            var value = BinaryPrimitives.ReadInt64LittleEndian(span);
            span = span[sizeof(long)..];
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ulong ReadUInt64()
        {
            var value = BinaryPrimitives.ReadUInt64LittleEndian(span);
            span = span[sizeof(ulong)..];
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float ReadSingle()
        {
            return BitConverter.Int32BitsToSingle(span.ReadInt32());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double ReadDouble()
        {
            return BitConverter.Int64BitsToDouble(span.ReadInt64());
        }

#if NET5_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Half ReadHalf()
        {
            return BitConverter.Int16BitsToHalf(span.ReadInt16());
        }
#endif // NET5_0_OR_GREATER

#if NET6_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DateOnly ReadDateOnly()
        {
            return DateOnly.FromDayNumber(span.ReadInt32());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TimeOnly ReadTimeOnly()
        {
            return new TimeOnly(span.ReadInt64() / 100L); // nanoseconds since midnight
        }
#endif // NET6_0_OR_GREATER

#if NET7_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Int128 ReadInt128()
        {
            var value = new Int128(
                BinaryPrimitives.ReadUInt64LittleEndian(span[sizeof(ulong)..]),
                BinaryPrimitives.ReadUInt64LittleEndian(span));
            span = span[16..];
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UInt128 ReadUInt128()
        {
            var value = new UInt128(
                BinaryPrimitives.ReadUInt64LittleEndian(span[sizeof(ulong)..]),
                BinaryPrimitives.ReadUInt64LittleEndian(span));
            span = span[16..];
            return value;
        }
#endif // NET7_0_OR_GREATER

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private char ReadChar()
        {
            // UTF-8 encoded char is 1-4 bytes; determine byte count by checking the first byte
            int utf8ByteCount;
            byte b0 = span[0];
            if (b0 < 0x80)
            {
                utf8ByteCount = 1;
            }
            else if (b0 < 0xE0)
            {
                utf8ByteCount = 2;
            }
            else if (b0 < 0xF0)
            {
                utf8ByteCount = 3;
            }
            else
            {
                utf8ByteCount = 4;
            }

            Span<byte> buffer = stackalloc byte[utf8ByteCount];
            span[..utf8ByteCount].CopyTo(buffer);
#if NETSTANDARD2_0
            var value = Encoding.UTF8.GetString(buffer.ToArray())[0];
#else
            var value = Encoding.UTF8.GetString(buffer)[0];
#endif
            span = span[utf8ByteCount..];
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private decimal ReadDecimal()
        {
            // [0–11] little-endian UInt96 magnitude, [12] scale (0–28), [13] sign (0=+,1=-), [14–15] zero padding/reserved
            int lo = BinaryPrimitives.ReadInt32LittleEndian(span[..4]);
            int mid = BinaryPrimitives.ReadInt32LittleEndian(span[4..8]);
            int hi = BinaryPrimitives.ReadInt32LittleEndian(span[8..12]);
            int scale = span[12];
            bool sign = span[13] != 0;
            span = span[16..];
            return new decimal(lo, mid, hi, sign, (byte)scale);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Guid ReadGuid()
        {
#if NETSTANDARD
            var data = span[..16].ToArray();
            span = span[16..];
            Array.Reverse(data, 0, 4);
            Array.Reverse(data, 4, 2);
            Array.Reverse(data, 6, 2);
            return new Guid(data);
#else
            var data = span[..16];
            span = span[16..];
            return new Guid(data, true);
#endif // NETSTANDARD
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TimeSpan ReadTimeSpan(ProtoOptions options)
        {
            switch (options.TimeSpanEncoding)
            {
                case TimeSpanEncoding.Microseconds:
                    return TimeSpanHelper.FromMicroseconds(span.ReadInt64());

                case TimeSpanEncoding.Nanoseconds:
                    return TimeSpanHelper.FromNanoseconds(span.ReadInt64());

                case TimeSpanEncoding.Auto:
                    {
                        var kind = span.ReadByte();
                        return kind switch
                        {
                            MicrosecondsTag => TimeSpanHelper.FromMicroseconds(span.ReadInt64()),
                            NanosecondsTag => TimeSpanHelper.FromNanoseconds(span.ReadInt64()),
                            _ => throw new ProtobufSerializationException($"Timespan auto-encoding kind {kind} is not supported"),
                        };
                    }

                default:
                    throw new ProtobufSerializationException($"Timespan encoding option {options.TimeSpanEncoding} is not supported");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DateTime ReadDateTime(ProtoOptions options)
        {
            return new DateTime(
                span.ReadDateTimeTicks(options),
                DateTimeKind.Utc);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DateTimeOffset ReadDateTimeOffset(ProtoOptions options)
        {
            return new DateTimeOffset(
                span.ReadDateTimeTicks(options),
                TimeSpan.FromMinutes(span.ReadInt16()));
        }

        private long ReadDateTimeTicks(ProtoOptions options)
        {
            switch (options.DateTimeEncoding)
            {
                case DateTimeEncoding.UnixMicroseconds:
                    return DateTime.TicksFromUnixMicroseconds(span.ReadInt64());

                case DateTimeEncoding.UnixNanoseconds:
                    return DateTime.TicksFromUnixNanoseconds(span.ReadInt64());

                case DateTimeEncoding.Auto:
                    {
                        var kind = span.ReadByte();
                        return kind switch
                        {
                            MicrosecondsTag => DateTime.TicksFromUnixMicroseconds(span.ReadInt64()),
                            NanosecondsTag => DateTime.TicksFromUnixNanoseconds(span.ReadInt64()),
                            _ => throw new ProtobufSerializationException($"Datetime auto-encoding kind {kind} is not supported"),
                        };
                    }

                default:
                    throw new ProtobufSerializationException($"Datetime encoding option {options.DateTimeEncoding} is not supported");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BigInteger ReadBigInteger()
        {
            var length = span.ReadInt32();
#if NETSTANDARD2_0
            var bytes = span[..length].ToArray();
#else
            var bytes = span[..length];
#endif // NETSTANDARD2_0
            span = span[length..];
            return new BigInteger(bytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Complex ReadComplex()
        {
            return new Complex(
                span.ReadDouble(),
                span.ReadDouble());
        }
    }
}

#if NETSTANDARD2_0
file static class BitConverter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int SingleToInt32Bits(float value) => Unsafe.As<float, int>(ref value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Int32BitsToSingle(int value) => Unsafe.As<int, float>(ref value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long DoubleToInt64Bits(double value) => Unsafe.As<double, long>(ref value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Int64BitsToDouble(long value) => Unsafe.As<long, double>(ref value);
}
#endif // NETSTANDARD2_0
