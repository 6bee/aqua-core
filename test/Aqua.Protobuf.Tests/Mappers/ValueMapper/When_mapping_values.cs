// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Tests.Mappers.ValueMapper;

using Aqua.Dynamic;
using Aqua.Protobuf.Mappers;
using Aqua.TypeSystem;
using System.Collections;
using System.Numerics;

public sealed class When_mapping_values
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
    public void Should_round_trip_null() => MapperTestHelper.RoundTrip(ValueMapper.Instance, default(object)).ShouldBeNull();

    [Theory]
    [InlineData("")]
    [InlineData("aqua")]
    public void Should_round_trip_string_values(string value)
    {
        var result = MapperTestHelper.RoundTrip(ValueMapper.Instance, value);

        result.ShouldBe(value);
    }

    [Theory]
    [MemberData(nameof(ScalarValues))]
    public void Should_round_trip_scalar_values(object value)
    {
        var result = MapperTestHelper.RoundTrip(ValueMapper.Instance, value);

        result.ShouldBe(value);
        result.GetType().ShouldBe(value.GetType());
    }

    [Fact]
    public void Should_round_trip_packed_primitive_array()
    {
        var result = MapperTestHelper.RoundTrip(ValueMapper.Instance, new[] { 1, 2, 3 });

        result.ShouldBeOfType<int[]>();
        ((int[])result).ShouldBe([1, 2, 3]);
    }

    [Fact]
    public void Should_round_trip_heterogeneous_collection()
    {
        var result = MapperTestHelper.RoundTrip(ValueMapper.Instance, (IEnumerable)new ArrayList { "a", 1 });

        result.ShouldBeOfType<object[]>();
        ((object[])result).ShouldBe(["a", 1]);
    }

    [Fact]
    public void Should_round_trip_null_bearing_array_as_collection()
    {
        var result = MapperTestHelper.RoundTrip(ValueMapper.Instance, new string[] { "a", null, "b" });

        result.ShouldBeOfType<object[]>();
        ((object[])result).ShouldBe(["a", null, "b"]);
    }

    [Fact]
    public void Should_round_trip_dynamic_object()
    {
        var dynamicObject = new DynamicObject([("Name", (object)"aqua")]);

        var dynamicResult = MapperTestHelper.RoundTrip(ValueMapper.Instance, dynamicObject);

        dynamicResult.ShouldBeOfType<DynamicObject>().Get<string>("Name").ShouldBe("aqua");
    }

    [Fact]
    public void Should_round_trip_type_and_member_information()
    {
        var type = new TypeInfo(typeof(string), false, false);
        var property = new PropertyInfo("Length", type, type);
        var field = new FieldInfo("Empty", type);
        var constructor = new ConstructorInfo { Name = ".ctor", DeclaringType = type };
        var method = new MethodInfo("ToString", type);

        var typeResult = MapperTestHelper.RoundTrip(ValueMapper.Instance, type);
        var propertyResult = MapperTestHelper.RoundTrip(ValueMapper.Instance, property);
        var fieldResult = MapperTestHelper.RoundTrip(ValueMapper.Instance, field);
        var constructorResult = MapperTestHelper.RoundTrip(ValueMapper.Instance, constructor);
        var methodResult = MapperTestHelper.RoundTrip(ValueMapper.Instance, method);

        typeResult.ShouldBeOfType<TypeInfo>().Name.ShouldBe("String");
        propertyResult.ShouldBeOfType<PropertyInfo>().Name.ShouldBe("Length");
        fieldResult.ShouldBeOfType<FieldInfo>().Name.ShouldBe("Empty");
        constructorResult.ShouldBeOfType<ConstructorInfo>().Name.ShouldBe(".ctor");
        methodResult.ShouldBeOfType<MethodInfo>().Name.ShouldBe("ToString");
    }

    [Fact]
    public void Should_throw_for_unsupported_scalar()
    {
        Should.Throw<ProtobufSerializationException>(() => ValueMapper.Instance.ToProto(new Version(1, 2), ProtoContext.ForWrite()));
    }
}
