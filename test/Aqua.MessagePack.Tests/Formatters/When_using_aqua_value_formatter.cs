// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Tests.Formatters;

using Aqua.Dynamic;
using Aqua.MessagePack.Formatters;
using Aqua.TypeSystem;
using global::MessagePack;
using System.Collections;
using System.Buffers;
using System.Numerics;

public class When_using_aqua_value_formatter
{
    public static IEnumerable<object[]> ScalarValues()
    {
        yield return [true];
        yield return [(byte)1];
        yield return [(sbyte)-1];
        yield return [(short)-2];
        yield return [(ushort)2];
        yield return [-3];
        yield return [3U];
        yield return [-4L];
        yield return [4UL];
        yield return [1.25F];
        yield return [2.5D];
        yield return ['a'];
        yield return [3.75M];
        yield return [new BigInteger(42)];
        yield return [new Complex(2, 3)];
        yield return [Guid.Parse("5a1426d5-0296-424c-88e5-f4e4f121e5a7")];
        yield return [new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc)];
        yield return [new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.FromHours(1))];
        yield return [TimeSpan.FromMinutes(5)];
#if NET5_0_OR_GREATER
        yield return [(Half)1.5F];
#endif
#if NET6_0_OR_GREATER
        yield return [new DateOnly(2024, 1, 2)];
        yield return [new TimeOnly(3, 4, 5)];
#endif
#if NET7_0_OR_GREATER
        yield return [(Int128)(-123)];
        yield return [(UInt128)456];
#endif
    }

    [Fact]
    public void Null_should_round_trip()
    {
        var result = FormatterTestHelper.RoundTrip(AquaValueFormatter.Instance, default(object));

        result.ShouldBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("aqua")]
    public void String_should_round_trip(string value)
    {
        var result = FormatterTestHelper.RoundTrip(AquaValueFormatter.Instance, value);

        result.ShouldBe(value);
    }

    [Theory]
    [MemberData(nameof(ScalarValues))]
    public void Scalar_should_round_trip(object value)
    {
        var result = FormatterTestHelper.RoundTrip(AquaValueFormatter.Instance, value);

        result.ShouldBe(value);
        result.GetType().ShouldBe(value.GetType());
    }

    [Fact]
    public void Null_string_payload_should_deserialize_as_null()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new MessagePackWriter(buffer);
        writer.WriteArrayHeader(2);
        writer.Write((byte)2);
        writer.WriteNil();
        writer.Flush();

        var result = FormatterTestHelper.Decode(AquaValueFormatter.Instance, buffer.WrittenSpan.ToArray());

        result.ShouldBeNull();
    }

    [Fact]
    public void Packed_primitive_array_should_round_trip()
    {
        var result = FormatterTestHelper.RoundTrip(AquaValueFormatter.Instance, new[] { 1, 2, 3 });

        result.ShouldBeOfType<int[]>();
        ((int[])result).ShouldBe([1, 2, 3]);
    }

    [Fact]
    public void Null_bearing_array_should_round_trip_as_collection()
    {
        var result = FormatterTestHelper.RoundTrip(AquaValueFormatter.Instance, new string[] { "a", null, "b" });

        result.ShouldBeOfType<object[]>();
        ((object[])result).ShouldBe(["a", null, "b"]);
    }

    [Fact]
    public void Enumerable_should_round_trip_as_collection()
    {
        var result = FormatterTestHelper.RoundTrip(AquaValueFormatter.Instance, (IEnumerable)new ArrayList { "a", 1 });

        result.ShouldBeOfType<object[]>();
        ((object[])result).ShouldBe(["a", 1]);
    }

    [Fact]
    public void Dynamic_object_should_round_trip()
    {
        var value = new DynamicObject(new[] { ("Name", (object)"aqua") });

        var result = FormatterTestHelper.RoundTrip(AquaValueFormatter.Instance, value);

        result.ShouldBeOfType<DynamicObject>();
        ((DynamicObject)result).Get<string>("Name").ShouldBe("aqua");
    }

    [Fact]
    public void Type_and_member_information_should_round_trip()
    {
        var type = new TypeInfo(typeof(string), false, false);
        var field = new FieldInfo("Length", type);

        FormatterTestHelper.RoundTrip(AquaValueFormatter.Instance, type).ShouldBeOfType<TypeInfo>();
        FormatterTestHelper.RoundTrip(AquaValueFormatter.Instance, field).ShouldBeOfType<FieldInfo>();
    }

    [Fact]
    public void Unsupported_scalar_should_throw()
    {
        Should.Throw<MessagePackSerializationException>(() => FormatterTestHelper.Encode(AquaValueFormatter.Instance, new Version(1, 2)));
    }

    [Fact]
    public void Unknown_tag_should_throw()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new MessagePackWriter(buffer);
        writer.WriteArrayHeader(1);
        writer.Write((byte)255);
        writer.Flush();

        Should.Throw<MessagePackSerializationException>(() => FormatterTestHelper.Decode(AquaValueFormatter.Instance, buffer.WrittenSpan.ToArray()));
    }
}
