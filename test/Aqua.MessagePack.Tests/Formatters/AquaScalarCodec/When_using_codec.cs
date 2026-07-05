// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Tests.Formatters.AquaScalarCodec;

using Aqua.MessagePack.Formatters;
using System.Buffers;

public class When_using_codec
{
    [Theory]
    [MemberData(nameof(TestValues))]
    public async Task Should_rountrip(object value)
    {
        // encode
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new MessagePackWriter(buffer);

        AquaScalarCodec.Write(ref writer, value);

        var encoded = buffer.WrittenSpan;

        // decode
        var reader = new MessagePackReader(buffer.GetMemory());
        var type = value.GetType();
        var decoded = AquaScalarCodec.Read(ref reader, type);

        // assert
        decoded.ShouldBe(value, $"type: {type}");
    }

    public static IEnumerable<object[]> TestValues
    {
        get
        {
            // Core integral types
            yield return [true];
            yield return [false];
            yield return [(byte)255];
            yield return [(sbyte)-128];
            yield return ['A'];
            yield return [(short)-32768];
            yield return [(short)32767];
            yield return [(ushort)65535];
            yield return [-2147483648];
            yield return [2147483647];
            yield return [4294967295U];
            yield return [-9223372036854775808L];
            yield return [9223372036854775807L];
            yield return [18446744073709551615UL];

            // Floating point types
            yield return [1.5f];
            yield return [-1.5f];
            yield return [0.0f];
            yield return [-0.0f];
            yield return [float.MaxValue];
            yield return [float.MinValue];
            yield return [3.14159265358979];
            yield return [-3.14159265358979];
            yield return [0.0];
            yield return [-0.0];
            yield return [double.MaxValue];
            yield return [double.MinValue];

            // Decimal
            yield return [123.456m];
            yield return [-123.456m];
            yield return [0m];
            yield return [1.0m / 3.0m];
            yield return [decimal.MaxValue];
            yield return [decimal.MinValue];

            // Guid
            yield return [Guid.Empty];
            yield return [new Guid("6f4b5c3d-2e1a-4f5b-8c9d-0e1f2a3b4c5d")];

            // DateTime
            yield return [DateTime.UnixEpoch];
            yield return [DateTime.MaxValue];
            yield return [DateTime.MinValue];
            yield return [new DateTime(2023, 7, 15, 10, 30, 45, 999, DateTimeKind.Unspecified)];
            yield return [new DateTime(2023, 7, 15, 10, 30, 45, 999, DateTimeKind.Utc)];

            // DateTimeOffset
            yield return [new DateTimeOffset(2023, 7, 16, 10, 30, 45, TimeSpan.Zero)];
            yield return [new DateTimeOffset(new DateTime(2023, 7, 16, 10, 30, 45, DateTimeKind.Local))];
            yield return [new DateTimeOffset(new DateTime(2023, 7, 16, 10, 30, 45, DateTimeKind.Unspecified), TimeSpan.FromHours(.25))];
            yield return [new DateTimeOffset(new DateTime(2023, 7, 16, 10, 30, 45, DateTimeKind.Unspecified), TimeSpan.FromHours(3))];
            yield return [new DateTimeOffset(new DateTime(2023, 7, 16, 10, 30, 45, DateTimeKind.Unspecified), TimeSpan.FromHours(-2))];
            yield return [new DateTimeOffset(2023, 7, 16, 10, 30, 45, TimeSpan.FromHours(-5))];

            // TimeSpan
            yield return [TimeSpan.Zero];
            yield return [TimeSpan.MaxValue];
            yield return [TimeSpan.MinValue];
            yield return [TimeSpan.FromTicks(1234567890)];
            yield return [TimeSpan.FromMilliseconds(123.456)];
            yield return [TimeSpan.FromMilliseconds(123.4567)];

            // BigInteger
            yield return [new System.Numerics.BigInteger(0)];
            yield return [new System.Numerics.BigInteger(123456789)];
            yield return [new System.Numerics.BigInteger(-123456789)];
            yield return [new System.Numerics.BigInteger(long.MaxValue)];
            yield return [new System.Numerics.BigInteger(long.MinValue)];
            yield return [new System.Numerics.BigInteger(1) << 64];
            yield return [-(new System.Numerics.BigInteger(1) << 64)];

            // Complex
            yield return [new System.Numerics.Complex(0, 0)];
            yield return [new System.Numerics.Complex(1.5, 2.5)];
            yield return [new System.Numerics.Complex(-1.5, -2.5)];
            yield return [new System.Numerics.Complex(1.0, 0.0)];

#if NET5_0_OR_GREATER
            // Half (NET5+)
            yield return [(Half)0x00FF];
            yield return [(Half)7.33E-06];
            yield return [(Half)0.0f];
            yield return [(Half)1.5f];
            yield return [(Half)(-1.5f)];
            yield return [Half.MaxValue];
            yield return [Half.MinValue];
#endif

#if NET6_0_OR_GREATER
            // DateOnly (NET6+)
            yield return [DateOnly.FromDayNumber(0)];
            yield return [DateOnly.FromDateTime(new DateTime(2023, 7, 15))];
            yield return [DateOnly.MaxValue];
            yield return [DateOnly.MinValue];

            // TimeOnly (NET6+)
            yield return [TimeOnly.MinValue];
            yield return [TimeOnly.FromDateTime(new DateTime(1, 1, 1, 10, 30, 45))];
            yield return [TimeOnly.FromDateTime(new DateTime(2023, 7, 15, 10, 30, 45))];
            yield return [TimeOnly.MaxValue];
#endif

#if NET7_0_OR_GREATER
            // Int128 (NET7+)
            yield return [new Int128(1234567890123456789UL, 0UL)];
            yield return [new Int128(0UL, 0UL)];
            yield return [new Int128(0UL, 1234567890123456789UL)];
            yield return [Int128.MaxValue];
            yield return [Int128.MinValue];

            // UInt128 (NET7+)
            yield return [new UInt128(0UL, 0UL)];
            yield return [new UInt128(1234567890123456789UL, 0UL)];
            yield return [UInt128.MaxValue];
#endif
        }
    }
}
