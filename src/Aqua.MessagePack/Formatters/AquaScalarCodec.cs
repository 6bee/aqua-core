// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Formatters;

using Aqua.TypeExtensions;
using System.Buffers;
using System.Buffers.Binary;
using System.Numerics;

/// <summary>
/// Deterministic MessagePack encoding for the scalar leaf types.
/// </summary>
/// <remarks>
/// Each scalar is written using MessagePack-native primitives where a stable mapping exists, and
/// explicit encodings for types not covered natively (e.g. <see cref="BigInteger"/>,
/// <see cref="Complex"/>, and version-gated numeric/date types). The element type is supplied by
/// the caller (resolved from the registry type key), so no type metadata is embedded here.
/// </remarks>
internal static class AquaScalarCodec
{
    private static class ExtType
    {
        public const sbyte Decimal = 1;
        public const sbyte Guid = 2;
#if NET7_0_OR_GREATER
        public const sbyte Int128 = 3;
        public const sbyte UInt128 = 4;
#endif // NET7_0_OR_GREATER
#if NET5_0_OR_GREATER
        public const sbyte Half = 5;
#endif // NET5_0_OR_GREATER
        public const sbyte BigInteger = 6;
        public const sbyte Complex = 7;
        ////public const sbyte DateTime = ReservedMessagePackExtensionTypeCode.DateTime;
        public const sbyte DateTimeOffset = 8;
        public const sbyte TimeSpan = 9;
#if NET6_0_OR_GREATER
        public const sbyte DateOnly = 10;
        public const sbyte TimeOnly = 11;
#endif // NET6_0_OR_GREATER
    }

    private static class TimeConstants
    {
        public const long NanosecondsPerSecond = 1_000_000_000;
        public const long NanosecondsPerTick = 100;
    }

    public static void Write(ref MessagePackWriter writer, object value)
    {
        switch (value)
        {
            case bool v: writer.Write(v); return;
            case byte v: writer.Write(v); return;
            case sbyte v: writer.Write(v); return;
            case short v: writer.Write(v); return;
            case ushort v: writer.Write(v); return;
            case int v: writer.Write(v); return;
            case uint v: writer.Write(v); return;
            case long v: writer.Write(v); return;
            case ulong v: writer.Write(v); return;
            case float v: writer.Write(v); return;
            case double v: writer.Write(v); return;
            case char v: writer.Write(v); return;
            case decimal v: writer.Write(v); return;
            case Guid v: writer.Write(v); return;
            case DateTime v: writer.Write(v); return;
            case DateTimeOffset v: writer.Write(v); return;
            case TimeSpan v: writer.Write(v); return;
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
        throw SerializationException($"Scalar type '{type}' is not supported by the Aqua MessagePack scalar codec.");
    }

    public static object Read(ref MessagePackReader reader, Type type)
    {
        type = type.AsNonNullableType();

        if (type == typeof(bool))
        {
            return reader.ReadBoolean();
        }

        if (type == typeof(byte))
        {
            return reader.ReadByte();
        }

        if (type == typeof(sbyte))
        {
            return reader.ReadSByte();
        }

        if (type == typeof(short))
        {
            return reader.ReadInt16();
        }

        if (type == typeof(ushort))
        {
            return reader.ReadUInt16();
        }

        if (type == typeof(int))
        {
            return reader.ReadInt32();
        }

        if (type == typeof(uint))
        {
            return reader.ReadUInt32();
        }

        if (type == typeof(long))
        {
            return reader.ReadInt64();
        }

        if (type == typeof(ulong))
        {
            return reader.ReadUInt64();
        }

        if (type == typeof(float))
        {
            return reader.ReadSingle();
        }

        if (type == typeof(double))
        {
            return reader.ReadDouble();
        }

        if (type == typeof(char))
        {
            return reader.ReadChar();
        }

        if (type == typeof(decimal))
        {
            return reader.ReadDecimal();
        }

        if (type == typeof(Guid))
        {
            return reader.ReadGuid();
        }

        if (type == typeof(DateTime))
        {
            return reader.ReadDateTime();
        }

        if (type == typeof(DateTimeOffset))
        {
            return reader.ReadDateTimeOffset();
        }

        if (type == typeof(TimeSpan))
        {
            return reader.ReadTimeSpan();
        }

        if (type == typeof(BigInteger))
        {
            return reader.ReadBigInteger();
        }

        if (type == typeof(Complex))
        {
            return reader.ReadComplex();
        }

#if NET5_0_OR_GREATER
        if (type == typeof(Half))
        {
            return reader.ReadHalf();
        }
#endif // NET5_0_OR_GREATER
#if NET6_0_OR_GREATER
        if (type == typeof(DateOnly))
        {
            return reader.ReadDateOnly();
        }

        if (type == typeof(TimeOnly))
        {
            return reader.ReadTimeOnly();
        }
#endif // NET6_0_OR_GREATER
#if NET7_0_OR_GREATER
        if (type == typeof(Int128))
        {
            return reader.ReadInt128();
        }

        if (type == typeof(UInt128))
        {
            return reader.ReadUInt128();
        }
#endif // NET7_0_OR_GREATER

        throw SerializationException($"Scalar type '{type}' is not supported by the Aqua MessagePack scalar codec.");
    }

    extension(ref MessagePackWriter writer)
    {
        private void Write(decimal value)
        {
            // Offset  Size   Meaning
            // ------  ----   --------------------------
            // 0       1      scale (0–28)
            // 1       1      sign (0 = +, 1 = -)
            // 2-13    12     96-bit integer (big-endian)

            // 1B scale | 1B sign | 12B BE magnitude
#if NETSTANDARD2_0
            var bits = decimal.GetBits(value);
#else
            Span<int> bits = stackalloc int[4];
            _ = decimal.GetBits(value, bits);
#endif // NETSTANDARD2_0

            byte scale = unchecked((byte)(bits[3] >> 16));
            byte sign = (bits[3] & unchecked((int)0x80000000)) != 0 ? (byte)1 : (byte)0;
            int hi = bits[2];
            int mid = bits[1];
            int low = bits[0];

            var size =
                hi != 0 ? 14 :
                mid != 0 ? 10 :
                low != 0 ? 6 :
                1;

            writer.WriteExtensionFormatHeader(new(ExtType.Decimal, size));
            var span = writer.GetSpan(size);

            switch (size)
            {
                case 14:
                    {
                        span[0] = scale;
                        span[1] = sign;

                        // write 96-bit magnitude (big-endian)
                        BinaryPrimitives.WriteInt32BigEndian(span[2..6], hi);
                        BinaryPrimitives.WriteInt32BigEndian(span[6..10], mid);
                        BinaryPrimitives.WriteInt32BigEndian(span[10..14], low);
                        break;
                    }

                case 10:
                    {
                        span[0] = scale;
                        span[1] = sign;

                        // write 64-bit magnitude (big-endian)
                        BinaryPrimitives.WriteInt32BigEndian(span[2..6], mid);
                        BinaryPrimitives.WriteInt32BigEndian(span[6..10], low);
                        break;
                    }

                case 6:
                    {
                        span[0] = scale;
                        span[1] = sign;

                        // write 32-bit magnitude (big-endian)
                        BinaryPrimitives.WriteInt32BigEndian(span[2..6], low);
                        break;
                    }

                case 1:
                    {
                        // for 0m we write a single byte as 0
                        span[0] = 0;
                        break;
                    }
            }

            writer.Advance(size);
        }

        private void Write(Complex value)
        {
            var size = value.Imaginary == 0 ? 8 : 16;
            writer.WriteExtensionFormatHeader(new(ExtType.Complex, size));

            var span = writer.GetSpan(size);

            BinaryPrimitives.WriteInt64BigEndian(span, BitConverter.DoubleToInt64Bits(value.Real));

            if (value.Imaginary != 0)
            {
                BinaryPrimitives.WriteInt64BigEndian(span[8..], BitConverter.DoubleToInt64Bits(value.Imaginary));
            }

            writer.Advance(size);
        }

        private void Write(BigInteger value)
        {
#if NETSTANDARD2_0
            var array = value.ToByteArray();
            var size = array.Length;
            writer.WriteExtensionFormatHeader(new(ExtType.BigInteger, size));
            var span = writer.GetSpan(size);
            array.CopyTo(span);
#else
            var size = value.GetByteCount();
            writer.WriteExtensionFormatHeader(new(ExtType.BigInteger, size));
            var span = writer.GetSpan(size);
            _ = value.TryWriteBytes(span, out _);
#endif // NETSTANDARD2_0
            writer.Advance(size);
        }

        private void Write(Guid value)
        {
            writer.WriteExtensionFormatHeader(new(ExtType.Guid, 16));
            var span = writer.GetSpan(16);
#if NETSTANDARD
            var bytes = value.ToByteArray();

            // Convert .NET Guid layout to RFC 4122 layout.
            Array.Reverse(bytes, 0, 4);
            Array.Reverse(bytes, 4, 2);
            Array.Reverse(bytes, 6, 2);

            bytes.CopyTo(span);
#else
            _ = value.TryWriteBytes(span, true, out _);
#endif // NETSTANDARD
            writer.Advance(16);
        }

        private void Write(TimeSpan value)
        {
            long ticks = value.Ticks;

            // --- Case 1: milliseconds (small + common) ---
            if (ticks % TimeSpan.TicksPerMillisecond == 0)
            {
                long ms = ticks / TimeSpan.TicksPerMillisecond;

                if (ms >= int.MinValue && ms <= int.MaxValue)
                {
                    writer.WriteExtensionFormatHeader(new(ExtType.TimeSpan, 4));

                    Span<byte> span = writer.GetSpan(4);
                    BinaryPrimitives.WriteInt32BigEndian(span, (int)ms);
                    writer.Advance(4);

                    return;
                }
            }

            // --- Case 2: nanoseconds (high precision compact) ---
            // 1 tick = 100 ns
            bool fitsNanoseconds =
                ticks <= long.MaxValue / TimeConstants.NanosecondsPerTick &&
                ticks >= long.MinValue / TimeConstants.NanosecondsPerTick;
            if (fitsNanoseconds)
            {
                long ns = ticks * TimeConstants.NanosecondsPerTick;

                writer.WriteExtensionFormatHeader(new(ExtType.TimeSpan, 8));

                Span<byte> span = writer.GetSpan(8);
                BinaryPrimitives.WriteInt64BigEndian(span, ns);
                writer.Advance(8);

                return;
            }

            // --- Case 3: seconds + nanos (fallback, widest range) ---
            long seconds = ticks / TimeSpan.TicksPerSecond;
            long remainderTicks = ticks - (seconds * TimeSpan.TicksPerSecond);
            if (remainderTicks < 0)
            {
                seconds--;
                remainderTicks += TimeSpan.TicksPerSecond;
            }

            uint nanos = (uint)(remainderTicks * TimeConstants.NanosecondsPerTick);

            writer.WriteExtensionFormatHeader(new(ExtType.TimeSpan, 12));

            Span<byte> span12 = writer.GetSpan(12);

            BinaryPrimitives.WriteInt64BigEndian(span12[..8], seconds);
            BinaryPrimitives.WriteUInt32BigEndian(span12[8..12], nanos);

            writer.Advance(12);
        }

        private void Write(DateTimeOffset value)
        {
            Span<byte> buffer = stackalloc byte[15 + 3];

            // We're writing a *local* DateTime value in msgpack encoding as if it were UTC time as is done by MessagePack formater for DateTimeOffset
            // This does not strictly match the MessagePack spec’s intended timestamp semantics, but preserves both the UTC instant and the original DateTimeOffset offset.
            MessagePackPrimitives.TryWrite(buffer, new DateTime(value.Ticks, DateTimeKind.Utc), out var size);

            var offsetMinutes = (short)value.Offset.TotalMinutes;
            switch (offsetMinutes)
            {
                case > 0 and <= byte.MaxValue:
                    {
                        MessagePackPrimitives.TryWriteUInt8(buffer[size..], (byte)offsetMinutes, out var sizeOffset);
                        size += sizeOffset;
                        break;
                    }

                case >= sbyte.MinValue and <= sbyte.MaxValue:
                    {
                        MessagePackPrimitives.TryWriteInt8(buffer[size..], (sbyte)offsetMinutes, out var sizeOffset);
                        size += sizeOffset;
                        break;
                    }

                case not 0:
                    {
                        MessagePackPrimitives.TryWriteInt16(buffer[size..], offsetMinutes, out var sizeOffset);
                        size += sizeOffset;
                        break;
                    }
            }

            writer.WriteExtensionFormatHeader(new(ExtType.DateTimeOffset, size));

            var span = writer.GetSpan(size);
            buffer.CopyTo(span);
            writer.Advance(size);
        }

#if NET5_0_OR_GREATER
        private void Write(Half value)
        {
            ushort bits = BitConverter.HalfToUInt16Bits(value);
            var size = bits switch
            {
                <= byte.MaxValue => 1,
                _ => 2,
            };
            writer.WriteExtensionFormatHeader(new ExtensionHeader(ExtType.Half, size));

            var span = writer.GetSpan(size);
            switch (size)
            {
                case 1:
                    span[0] = (byte)bits;
                    break;

                default:
                    BinaryPrimitives.WriteUInt16BigEndian(span, bits);
                    break;
            }

            writer.Advance(size);
        }
#endif // NET5_0_OR_GREATER
#if NET6_0_OR_GREATER
        private void Write(DateOnly value)
        {
            writer.WriteExtensionFormatHeader(new ExtensionHeader(ExtType.DateOnly, 4));
            var span = writer.GetSpan(4);
            BinaryPrimitives.WriteInt32BigEndian(span, value.DayNumber);
            writer.Advance(4);
        }

        private void Write(TimeOnly value)
        {
            var nanoseconds = value.Ticks * 100L;
            writer.WriteExtensionFormatHeader(new ExtensionHeader(ExtType.TimeOnly, 8));
            var span = writer.GetSpan(8);
            BinaryPrimitives.WriteInt64BigEndian(span, nanoseconds);
            writer.Advance(8);
        }
#endif // NET6_0_OR_GREATER
#if NET7_0_OR_GREATER
        private void Write(Int128 value)
        {
            var lo = (ulong)(value & ulong.MaxValue);
            var hi = (ulong)(value >> 64);

            var size = hi == 0 ? 8 : 16;
            writer.WriteExtensionFormatHeader(new ExtensionHeader(ExtType.Int128, size));
            var span = writer.GetSpan(size);
            if (hi != 0)
            {
                BinaryPrimitives.WriteUInt64BigEndian(span, hi);
                BinaryPrimitives.WriteUInt64BigEndian(span[8..], lo);
            }
            else
            {
                BinaryPrimitives.WriteUInt64BigEndian(span, lo);
            }

            writer.Advance(size);
        }

        private void Write(UInt128 value)
        {
            var lo = (ulong)(value & ulong.MaxValue);
            var hi = (ulong)(value >> 64);

            var size = hi == 0 ? 8 : 16;
            writer.WriteExtensionFormatHeader(new ExtensionHeader(ExtType.UInt128, size));
            var span = writer.GetSpan(size);
            if (hi != 0)
            {
                BinaryPrimitives.WriteUInt64BigEndian(span, hi);
                BinaryPrimitives.WriteUInt64BigEndian(span[8..], lo);
            }
            else
            {
                BinaryPrimitives.WriteUInt64BigEndian(span, lo);
            }

            writer.Advance(size);
        }
#endif // NET7_0_OR_GREATER
    }

    extension(ref MessagePackReader reader)
    {
        private ReadOnlySequence<byte> ReadExtension(sbyte type, long? size = null)
        {
            var ext = reader.ReadExtensionFormat();

            if (ext.TypeCode != type)
            {
                throw SerializationException($"Unexpected ext type {ext.TypeCode}");
            }

            if (size.HasValue && ext.Data.Length != size)
            {
                throw InvalidExtSizeException(type, ext.Data.Length);
            }

            return ext.Data;
        }

        private decimal ReadDecimal()
        {
            // [0] scale (0–28), [1] sign (0=+,1=-), [2-13] big-endian UInt96 magnitude
            var ext = reader.ReadExtension(ExtType.Decimal);
            switch (ext.Length)
            {
                case 14:
                    {
                        Span<byte> span = stackalloc byte[14];
                        ext.Read(span, "decimal");
                        byte scale = span[0];
                        bool sign = span[1] != 0;
                        int hi = BinaryPrimitives.ReadInt32BigEndian(span[2..6]);
                        int mid = BinaryPrimitives.ReadInt32BigEndian(span[6..10]);
                        int lo = BinaryPrimitives.ReadInt32BigEndian(span[10..14]);
                        return new decimal(lo, mid, hi, sign, scale);
                    }

                case 10:
                    {
                        Span<byte> span = stackalloc byte[10];
                        ext.Read(span, "decimal");
                        byte scale = span[0];
                        bool sign = span[1] != 0;
                        int mid = BinaryPrimitives.ReadInt32BigEndian(span[2..6]);
                        int lo = BinaryPrimitives.ReadInt32BigEndian(span[6..10]);
                        return new decimal(lo, mid, 0, sign, scale);
                    }

                case 6:
                    {
                        Span<byte> span = stackalloc byte[6];
                        ext.Read(span, "decimal");
                        byte scale = span[0];
                        bool sign = span[1] != 0;
                        int lo = BinaryPrimitives.ReadInt32BigEndian(span[2..6]);
                        return new decimal(lo, 0, 0, sign, scale);
                    }

                case 1:
                    {
                        Span<byte> span = stackalloc byte[1];
                        ext.Read(span, "decimal");
                        if (span[0] != 0)
                        {
                            throw SerializationException("Zero value expected");
                        }

                        return 0m;
                    }

                default:
                    throw InvalidExtSizeException(ExtType.TimeSpan, ext.Length);
            }
        }

        private BigInteger ReadBigInteger()
        {
            var ext = reader.ReadExtension(ExtType.BigInteger);
            Span<byte> span = ext.Length <= 256
                ? stackalloc byte[(int)ext.Length]
                : new byte[ext.Length];
            ext.Read(span, "bigint");
#if NETSTANDARD2_0
            return new BigInteger(span.ToArray());
#else
            return new BigInteger(span);
#endif // NETSTANDARD2_0
        }

        private Complex ReadComplex()
        {
            var ext = reader.ReadExtension(ExtType.Complex);
            switch (ext.Length)
            {
                case 16:
                    {
                        Span<byte> span = stackalloc byte[16];
                        ext.Read(span, "complex");
                        double real = BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64BigEndian(span));
                        double imaginary = BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64BigEndian(span[8..]));
                        return new Complex(real, imaginary);
                    }

                case 8:
                    {
                        Span<byte> span = stackalloc byte[8];
                        ext.Read(span, "complex");
                        double real = BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64BigEndian(span));
                        return new Complex(real, 0);
                    }

                default:
                    throw InvalidExtSizeException(ExtType.Complex, ext.Length);
            }
        }

        private Guid ReadGuid()
        {
            var ext = reader.ReadExtension(ExtType.Guid, 16);
            Span<byte> span = stackalloc byte[16];
            ext.Read(span, "guid");
#if NETSTANDARD2_0
            var data = span.ToArray();
            Array.Reverse(data, 0, 4);
            Array.Reverse(data, 4, 2);
            Array.Reverse(data, 6, 2);
            return new Guid(data);
#else
            return new Guid(span, true);
#endif // NETSTANDARD2_0
        }

        private TimeSpan ReadTimeSpan()
        {
            var ext = reader.ReadExtension(ExtType.TimeSpan);

            switch (ext.Length)
            {
                // --- milliseconds ---
                case 4:
                    {
                        Span<byte> span = stackalloc byte[4];
                        ext.Read(span, "milliseconds");
                        int ms = BinaryPrimitives.ReadInt32BigEndian(span);
                        return TimeSpan.FromMilliseconds(ms);
                    }

                // --- nanoseconds ---
                case 8:
                    {
                        Span<byte> span = stackalloc byte[8];
                        ext.Read(span, "nanoseconds");
                        long ns = BinaryPrimitives.ReadInt64BigEndian(span);
                        return TimeSpan.FromTicks(ns / TimeConstants.NanosecondsPerTick);
                    }

                // --- seconds + nanos ---
                case 12:
                    {
                        Span<byte> span = stackalloc byte[12];
                        ext.Read(span, "seconds + nanos");
                        long seconds = BinaryPrimitives.ReadInt64BigEndian(span[..8]);
                        uint nanos = BinaryPrimitives.ReadUInt32BigEndian(span[8..12]);

                        if (nanos >= TimeConstants.NanosecondsPerSecond)
                        {
                            throw SerializationException("Invalid nanoseconds.");
                        }

                        long ticks = (seconds * TimeSpan.TicksPerSecond) + (nanos / TimeConstants.NanosecondsPerTick);
                        return TimeSpan.FromTicks(ticks);
                    }

                default:
                    throw InvalidExtSizeException(ExtType.TimeSpan, ext.Length);
            }
        }

        private DateTimeOffset ReadDateTimeOffset()
        {
            var ext = reader.ReadExtension(ExtType.DateTimeOffset);
            if (ext.Length > 256)
            {
                throw InvalidExtSizeException(ExtType.DateTimeOffset, ext.Length);
            }

            Span<byte> span = stackalloc byte[(int)ext.Length];
            ext.Read(span, "datetimeoffset");

            MessagePackPrimitives.TryReadDateTime(span, out var dateTime, out var tockenSizeDate);

            short offsetMinutes;
            switch (span.Length - tockenSizeDate)
            {
                case 0:
                    {
                        offsetMinutes = 0;
                        break;
                    }

                case 2:
                    {
                        switch (span[tockenSizeDate])
                        {
                            case MessagePackCode.UInt8:
                                {
                                    MessagePackPrimitives.TryReadByte(span[tockenSizeDate..], out var offsetValue, out var _);
                                    offsetMinutes = offsetValue;
                                    break;
                                }

                            case MessagePackCode.Int8:
                                {
                                    MessagePackPrimitives.TryReadSByte(span[tockenSizeDate..], out var offsetValue, out var _);
                                    offsetMinutes = offsetValue;
                                    break;
                                }

                            default:
                                throw SerializationException($"Unexpected message pack code {span[tockenSizeDate]} for offset minutes");
                        }

                        break;
                    }

                case 3:
                    {
                        MessagePackPrimitives.TryReadInt16(span[tockenSizeDate..], out var offsetValue, out var _);
                        offsetMinutes = offsetValue;
                        break;
                    }

                default:
                    throw InvalidExtSizeException(ExtType.DateTimeOffset, ext.Length);
            }

            return new DateTimeOffset(dateTime.Ticks, TimeSpan.FromMinutes(offsetMinutes));
        }

#if NET5_0_OR_GREATER
        private Half ReadHalf()
        {
            var ext = reader.ReadExtension(ExtType.Half);

            Span<byte> span = stackalloc byte[(int)ext.Length];
            ext.Read(span, "half");

            ushort bits;
            switch (ext.Length)
            {
                case 1:
                    {
                        bits = span[0];
                        break;
                    }

                case 2:
                    {
                        bits = BinaryPrimitives.ReadUInt16BigEndian(span);
                        break;
                    }

                default:
                    throw InvalidExtSizeException(ExtType.Half, ext.Length);
            }

            return BitConverter.UInt16BitsToHalf(bits);
        }
#endif // NET5_0_OR_GREATER
#if NET6_0_OR_GREATER
        private DateOnly ReadDateOnly()
        {
            var ext = reader.ReadExtension(ExtType.DateOnly);

            Span<byte> span = stackalloc byte[(int)ext.Length];
            ext.Read(span, "date");

            int dayNumber;
            switch (ext.Length)
            {
                case 4:
                    {
                        dayNumber = BinaryPrimitives.ReadInt32BigEndian(span);
                        break;
                    }

                default:
                    throw InvalidExtSizeException(ExtType.DateOnly, ext.Length);
            }

            return DateOnly.FromDayNumber(dayNumber);
        }

        private TimeOnly ReadTimeOnly()
        {
            var ext = reader.ReadExtension(ExtType.TimeOnly);

            Span<byte> span = stackalloc byte[(int)ext.Length];
            ext.Read(span, "time");

            long nanoseconds;
            switch (ext.Length)
            {
                case 8:
                    {
                        nanoseconds = BinaryPrimitives.ReadInt64BigEndian(span);
                        break;
                    }

                default:
                    throw InvalidExtSizeException(ExtType.DateOnly, ext.Length);
            }

            return new TimeOnly(nanoseconds / 100L);
        }
#endif // NET6_0_OR_GREATER
#if NET7_0_OR_GREATER
        private Int128 ReadInt128()
        {
            var ext = reader.ReadExtension(ExtType.Int128);

            Span<byte> span = stackalloc byte[(int)ext.Length];
            ext.Read(span, "int128");

            ulong lo;
            ulong hi;
            switch (ext.Length)
            {
                case 16:
                    {
                        hi = BinaryPrimitives.ReadUInt64BigEndian(span);
                        lo = BinaryPrimitives.ReadUInt64BigEndian(span[8..]);
                        break;
                    }

                case 8:
                    {
                        hi = 0;
                        lo = BinaryPrimitives.ReadUInt64BigEndian(span);
                        break;
                    }

                default:
                    throw InvalidExtSizeException(ExtType.Int128, ext.Length);
            }

            return new Int128(hi, lo);
        }

        private UInt128 ReadUInt128()
        {
            var ext = reader.ReadExtension(ExtType.UInt128);

            Span<byte> span = stackalloc byte[(int)ext.Length];
            ext.Read(span, "uint128");

            ulong lo;
            ulong hi;
            switch (ext.Length)
            {
                case 16:
                    {
                        hi = BinaryPrimitives.ReadUInt64BigEndian(span);
                        lo = BinaryPrimitives.ReadUInt64BigEndian(span[8..]);
                        break;
                    }

                case 8:
                    {
                        hi = 0;
                        lo = BinaryPrimitives.ReadUInt64BigEndian(span);
                        break;
                    }

                default:
                    throw InvalidExtSizeException(ExtType.UInt128, ext.Length);
            }

            return new UInt128(hi, lo);
        }
#endif // NET7_0_OR_GREATER
    }

    extension(ReadOnlySequence<byte> sequence)
    {
        /// <summary>
        /// Copy <paramref name="sequence"/> into <paramref name="destination"/>.
        /// </summary>
        /// <param name="destination">The span to write to.</param>
        /// <param name="typeName">The type name to be given in a potential exception message.</param>
        /// <exception cref="MessagePackSerializationException">Thrown if <paramref name="sequence"/> and <paramref name="destination"/> are not equal in size.</exception>
        private void Read(scoped Span<byte> destination, string typeName)
        {
            if (sequence.Length != destination.Length)
            {
                throw SerializationException($"Invalid {typeName} payload length");
            }

            sequence.CopyTo(destination);
        }
    }

    private static MessagePackSerializationException InvalidExtSizeException(sbyte type, long size) => SerializationException($"Invalid ext type {type} extension size {size}");

    private static MessagePackSerializationException SerializationException(string message) => new(message);
}
