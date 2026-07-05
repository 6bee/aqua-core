// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Tests.PackedArraySerializer;

using Aqua.Protobuf;

public sealed class When_packing_and_unpacking_primitive_arrays
{
    [Theory]
    [InlineData(typeof(sbyte))]
    [InlineData(typeof(byte))]
    [InlineData(typeof(short))]
    [InlineData(typeof(ushort))]
    [InlineData(typeof(int))]
    [InlineData(typeof(uint))]
    [InlineData(typeof(long))]
    [InlineData(typeof(ulong))]
    [InlineData(typeof(float))]
    [InlineData(typeof(double))]
    [InlineData(typeof(bool))]
    [InlineData(typeof(char))]
    public void Should_report_element_type_as_eligible(Type elementType)
    {
        PackedArraySerializer.IsEligibleElementType(elementType).ShouldBeTrue();
    }

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(decimal))]
    [InlineData(typeof(DateTime))]
    [InlineData(typeof(Guid))]
    [InlineData(typeof(int?))]
    public void Should_report_element_type_as_not_eligible(Type elementType)
    {
        PackedArraySerializer.IsEligibleElementType(elementType).ShouldBeFalse();
    }

    [Theory]
    [InlineData(typeof(sbyte))]
    [InlineData(typeof(byte))]
    [InlineData(typeof(short))]
    [InlineData(typeof(ushort))]
    [InlineData(typeof(int))]
    [InlineData(typeof(uint))]
    [InlineData(typeof(long))]
    [InlineData(typeof(ulong))]
    [InlineData(typeof(bool))]
    [InlineData(typeof(char))]
    public void Should_pack_and_unpack_empty_array(Type elementType)
    {
        var empty = Array.CreateInstance(elementType, 0);
        var bytes = PackedArraySerializer.Pack(empty);
        bytes.ShouldBeEmpty();
        var roundtripped = PackedArraySerializer.Unpack(bytes, elementType);
        roundtripped.Length.ShouldBe(0);
        roundtripped.GetType().GetElementType().ShouldBe(elementType);
    }

    [Fact]
    public void Should_pack_and_unpack_byte_array()
    {
        byte[] original = [0, 1, 127, 128, 255];
        var bytes = PackedArraySerializer.Pack(original);
        bytes.ShouldBe(original);
        var roundtripped = (byte[])PackedArraySerializer.Unpack(bytes, typeof(byte));
        roundtripped.ShouldBe(original);
    }

    [Fact]
    public void Should_pack_and_unpack_sbyte_array()
    {
        sbyte[] original = [-128, -1, 0, 1, 127];
        var bytes = PackedArraySerializer.Pack(original);
        bytes.Length.ShouldBe(5);
        var roundtripped = (sbyte[])PackedArraySerializer.Unpack(bytes, typeof(sbyte));
        roundtripped.ShouldBe(original);
    }

    [Fact]
    public void Should_pack_and_unpack_short_array_little_endian()
    {
        short[] original = [0x0102, -1];
        var bytes = PackedArraySerializer.Pack(original);
        bytes.Length.ShouldBe(4);
        bytes[0].ShouldBe((byte)0x02); // low byte first
        bytes[1].ShouldBe((byte)0x01); // high byte second
        var roundtripped = (short[])PackedArraySerializer.Unpack(bytes, typeof(short));
        roundtripped.ShouldBe(original);
    }

    [Fact]
    public void Should_pack_and_unpack_int_array()
    {
        int[] original = [int.MinValue, -1, 0, 1, int.MaxValue];
        var bytes = PackedArraySerializer.Pack(original);
        bytes.Length.ShouldBe(20);
        var roundtripped = (int[])PackedArraySerializer.Unpack(bytes, typeof(int));
        roundtripped.ShouldBe(original);
    }

    [Fact]
    public void Should_pack_and_unpack_long_array()
    {
        long[] original = [long.MinValue, -1L, 0L, 1L, long.MaxValue];
        var bytes = PackedArraySerializer.Pack(original);
        bytes.Length.ShouldBe(40);
        var roundtripped = (long[])PackedArraySerializer.Unpack(bytes, typeof(long));
        roundtripped.ShouldBe(original);
    }

    [Fact]
    public void Should_pack_and_unpack_float_array()
    {
        float[] original = [float.NegativeInfinity, -1.5f, 0f, 1.5f, float.PositiveInfinity, float.NaN];
        var bytes = PackedArraySerializer.Pack(original);
        bytes.Length.ShouldBe(24);
        var roundtripped = (float[])PackedArraySerializer.Unpack(bytes, typeof(float));
        roundtripped.Length.ShouldBe(original.Length);
        for (var i = 0; i < original.Length; i++)
        {
            if (float.IsNaN(original[i]))
            {
                float.IsNaN(roundtripped[i]).ShouldBeTrue();
            }
            else
            {
                roundtripped[i].ShouldBe(original[i]);
            }
        }
    }

    [Fact]
    public void Should_pack_and_unpack_double_array()
    {
        double[] original = [double.NegativeInfinity, -1.5, 0.0, 1.5, double.PositiveInfinity, double.NaN];
        var bytes = PackedArraySerializer.Pack(original);
        bytes.Length.ShouldBe(48);
        var roundtripped = (double[])PackedArraySerializer.Unpack(bytes, typeof(double));
        roundtripped.Length.ShouldBe(original.Length);
        for (var i = 0; i < original.Length; i++)
        {
            if (double.IsNaN(original[i]))
            {
                double.IsNaN(roundtripped[i]).ShouldBeTrue();
            }
            else
            {
                roundtripped[i].ShouldBe(original[i]);
            }
        }
    }

    [Fact]
    public void Should_pack_and_unpack_bool_array()
    {
        bool[] original = [false, true, false, true];
        var bytes = PackedArraySerializer.Pack(original);
        bytes.ShouldBe(new byte[] { 0, 1, 0, 1 });
        var roundtripped = (bool[])PackedArraySerializer.Unpack(bytes, typeof(bool));
        roundtripped.ShouldBe(original);
    }

    [Fact]
    public void Should_pack_and_unpack_char_array()
    {
        char[] original = ['A', '\0', '\uFFFF'];
        var bytes = PackedArraySerializer.Pack(original);
        bytes.Length.ShouldBe(6);
        bytes[0].ShouldBe((byte)0x41); // 'A' low byte
        bytes[1].ShouldBe((byte)0x00); // 'A' high byte
        var roundtripped = (char[])PackedArraySerializer.Unpack(bytes, typeof(char));
        roundtripped.ShouldBe(original);
    }

    [Fact]
    public void Should_throw_on_pack_null()
    {
        Should.Throw<ArgumentNullException>(() => PackedArraySerializer.Pack(null!));
    }

    [Fact]
    public void Should_throw_on_pack_ineligible_element_type()
    {
        var array = new decimal[] { 1m };
        Should.Throw<ArgumentException>(() => PackedArraySerializer.Pack(array));
    }

    [Fact]
    public void Should_throw_on_unpack_null_bytes()
    {
        Should.Throw<ArgumentNullException>(() => PackedArraySerializer.Unpack(null!, typeof(int)));
    }

    [Fact]
    public void Should_throw_on_unpack_null_element_type()
    {
        Should.Throw<ArgumentNullException>(() => PackedArraySerializer.Unpack(Array.Empty<byte>(), null!));
    }

    [Fact]
    public void Should_throw_on_unpack_misaligned_payload()
    {
        var bytes = new byte[3]; // not aligned to int (4 bytes)
        Should.Throw<ArgumentException>(() => PackedArraySerializer.Unpack(bytes, typeof(int)));
    }

    [Fact]
    public void Should_throw_on_unpack_ineligible_element_type()
    {
        Should.Throw<ArgumentException>(() => PackedArraySerializer.Unpack(Array.Empty<byte>(), typeof(decimal)));
    }

    [Theory]
    [InlineData(typeof(sbyte))]
    [InlineData(typeof(byte))]
    [InlineData(typeof(short))]
    [InlineData(typeof(ushort))]
    [InlineData(typeof(int))]
    [InlineData(typeof(uint))]
    [InlineData(typeof(long))]
    [InlineData(typeof(ulong))]
    [InlineData(typeof(float))]
    [InlineData(typeof(double))]
    [InlineData(typeof(bool))]
    [InlineData(typeof(char))]
    public void Should_provide_element_width(Type elementType)
    {
        PackedArraySerializer.TryGetElementWidth(elementType, out var width).ShouldBeTrue();
        width.ShouldBeGreaterThan(0);
    }
}
