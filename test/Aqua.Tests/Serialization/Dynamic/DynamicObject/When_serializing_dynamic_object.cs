// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.Dynamic.DynamicObject;

using Aqua.Dynamic;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xunit;
using TypeInfo = Aqua.TypeSystem.TypeInfo;

public abstract class When_serializing_dynamic_object
{
#if !NET8_0_OR_GREATER
    public class With_binary_formatter : When_serializing_dynamic_object
    {
        public With_binary_formatter()
            : base(BinarySerializationHelper.Clone)
        {
        }
    }

#endif // NET8_0_OR_GREATER
    public class With_newtonsoft_json_serializer : When_serializing_dynamic_object
    {
        public With_newtonsoft_json_serializer()
            : base(NewtonsoftJsonSerializationHelper.Clone)
        {
        }
    }

    public class With_system_text_json_serializer : When_serializing_dynamic_object
    {
        public With_system_text_json_serializer()
            : base(SystemTextJsonSerializationHelper.Clone)
        {
        }
    }

    public class With_data_contract_serializer : When_serializing_dynamic_object
    {
        public With_data_contract_serializer()
            : base(DataContractSerializationHelper.Clone)
        {
        }
    }

#if NETFRAMEWORK
    public class With_net_data_contract_serializer : When_serializing_dynamic_object
    {
        public With_net_data_contract_serializer()
            : base(NetDataContractSerializationHelper.Clone)
        {
        }
    }
#endif // NETFRAMEWORK

    public class With_protobuf_net_serializer : When_serializing_dynamic_object
    {
        public With_protobuf_net_serializer()
            : base(ProtobufNetSerializationHelper.Clone)
        {
        }
    }

    public class With_xml_serializer : When_serializing_dynamic_object
    {
        public With_xml_serializer()
            : base(XmlSerializationHelper.Serialize)
        {
        }
    }

    private class A<T>
    {
        public T Value { get; set; }
    }

    private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;

    private readonly Func<DynamicObject, DynamicObject> _serialize;

    protected When_serializing_dynamic_object(Func<DynamicObject, DynamicObject> serialize)
    {
        _serialize = serialize;
    }

    [SkippableTheory]
    [MemberData(nameof(TestData.TestValues), MemberType = typeof(TestData))]
    [MemberData(nameof(TestData.TestValueArrays), MemberType = typeof(TestData))]
    [MemberData(nameof(TestData.TestValueLists), MemberType = typeof(TestData))]
    public void Should_serialize_value(Type type, object value, CultureInfo culture)
    {
        if (this.TestIs<With_protobuf_net_serializer>())
        {
            ProtobufNetSerializationHelper.SkipUnsupportedDataType(type, value);
        }

        if (this.TestIs<With_system_text_json_serializer>())
        {
            SystemTextJsonSerializationHelper.SkipUnsupportedDataType(type, value);
        }

#if NET5_0_OR_GREATER
        Skip.If(type.Is<Half>(), "Half type serialization is not supported.");
#endif // NET5_0_OR_GREATER
        Skip.If(this.TestIs<With_xml_serializer>(), "XmlSerializer has limited type support.");

        using var cultureContext = culture.CreateContext();

        var result = Serialize(type, value);
        result.ShouldBe(value, $"type: {type.FullName}");
    }

    [SkippableTheory]
    [MemberData(nameof(TestData.TestValues), MemberType = typeof(TestData))]
    [MemberData(nameof(TestData.TestValueArrays), MemberType = typeof(TestData))]
    [MemberData(nameof(TestData.TestValueLists), MemberType = typeof(TestData))]
    public void Should_serialize_value_when_using_string_formatting(Type type, object value, CultureInfo culture)
    {
        if (this.TestIs<With_protobuf_net_serializer>())
        {
            ProtobufNetSerializationHelper.SkipUnsupportedDataType(type, value);
        }

        if (this.TestIs<With_system_text_json_serializer>())
        {
            SystemTextJsonSerializationHelper.SkipUnsupportedDataType(type, value);
        }

        Skip.If(this.TestIs<With_xml_serializer>() && type.Is<char>(), "Only characters which are valid in xml may be supported by XmlSerializer.");

        using var cultureContext = culture.CreateContext();

        var result = Serialize(type, value, formatValuesAsStrings: true);
        result.ShouldBe(value, $"type: {type.FullName}");
    }

    [SkippableTheory]
    [MemberData(nameof(TestData.TestValues), MemberType = typeof(TestData))]
    [MemberData(nameof(TestData.TestValueArrays), MemberType = typeof(TestData))]
    [MemberData(nameof(TestData.TestValueLists), MemberType = typeof(TestData))]
    public void Should_serialize_value_as_property(Type type, object value, CultureInfo culture)
    {
        if (this.TestIs<With_protobuf_net_serializer>())
        {
            ProtobufNetSerializationHelper.SkipUnsupportedDataType(type, value);
        }

        if (this.TestIs<With_system_text_json_serializer>())
        {
            SystemTextJsonSerializationHelper.SkipUnsupportedDataType(type, value);
        }

#if NET5_0_OR_GREATER
        Skip.If(type.Is<Half>(), "Half type serialization is not supported.");
#endif // NET5_0_OR_GREATER
        Skip.If(this.TestIs<With_xml_serializer>(), "XmlSerializer has limited type support.");

        using var cultureContext = culture.CreateContext();

        var result = SerializeAsProperty(type, value);
        result.ShouldBe(value, $"type: {type.FullName}");
    }

    [SkippableTheory]
    [MemberData(nameof(TestData.TestValues), MemberType = typeof(TestData))]
    [MemberData(nameof(TestData.TestValueArrays), MemberType = typeof(TestData))]
    [MemberData(nameof(TestData.TestValueLists), MemberType = typeof(TestData))]
    public void Should_serialize_value_as_property_when_using_string_formatting(Type type, object value, CultureInfo culture)
    {
        Skip.If(this.TestIs<With_xml_serializer>() && type.Is<char>(), "Only characters which are valid in xml may be supported by XmlSerializer.");

        using var cultureContext = culture.CreateContext();

        var result = SerializeAsProperty(type, value, formatValuesAsStrings: true);
        result.ShouldBe(value, $"type: {type.FullName}  ({value})");
    }

    [Fact]
    public void Should_serialize_array_as_property()
    {
        var value = new[] { new TestData.RecordType { Value = "test" } };
        var result = SerializeAsProperty(value.GetType(), value);
        result
            .ShouldBeOfType<TestData.RecordType[]>()
            .ShouldHaveSingleItem()
            .Value.ShouldBe("test");
    }

    [Fact]
    public void Should_serialize_array()
    {
        var value = new[] { new TestData.RecordType { Value = "test" } };
        var result = Serialize(value.GetType(), value);
        result
            .ShouldBeOfType<TestData.RecordType[]>()
            .ShouldHaveSingleItem()
            .Value.ShouldBe("test");
    }

    [Fact]
    public void Should_serialize_empty_array_as_property()
    {
        var value = new TestData.RecordType[0];
        var result = SerializeAsProperty(value.GetType(), value);
        result.ShouldBe(value);
    }

    [Fact]
    public void Should_serialize_empty_array()
    {
        var value = new TestData.RecordType[0];
        var result = Serialize(value.GetType(), value);
        result.ShouldBe(value);
    }

    [Fact]
    public void Should_serialize_null_array()
    {
        var type = typeof(TestData.RecordType[]);
        var result = Serialize(type, null);
        result.ShouldBeNull();
    }

    [Fact]
    public void Should_serialize_null_array_as_property()
    {
        var type = typeof(TestData.RecordType[]);
        var result = SerializeAsProperty(type, null);
        result.ShouldBeNull();
    }

    [SkippableFact]
    public void Should_serialize_DateTimeOffset_as_property()
    {
        Skip.If(this.TestIs<With_protobuf_net_serializer>(), "DateTimeOffset is not supported by XmlSerializer.");
        Skip.If(this.TestIs<With_xml_serializer>(), "DateTimeOffset is not supported by XmlSerializer.");

        var value = new DateTimeOffset(2, 1, 2, 10, 0, 0, 300, new TimeSpan(1, 30, 0));
        var result = SerializeAsProperty(value.GetType(), value);
        result.ShouldBe(value);
    }

    [Fact]
    public void Should_serialize_DateTimeOffset_as_property_when_using_string_formatting()
    {
        var value = new DateTimeOffset(2, 1, 2, 10, 0, 0, 300, new TimeSpan(1, 30, 0));
        var result = SerializeAsProperty(value.GetType(), value, formatValuesAsStrings: true);
        result.ShouldBe(value);
    }

    [Fact]
    public void Should_serialize_DateTimeOffset_as_property_when_using_string_formatting2()
    {
        var mapperSettings = new DynamicObjectMapperSettings { FormatNativeTypesAsString = true };
        var mapper = new DynamicObjectMapper(mapperSettings);

        var dto1 = new DateTimeOffset(2, 1, 2, 10, 0, 0, 300, new TimeSpan(1, 30, 0));
        var dtoDynamic = mapper.MapObject(dto1);
        var dto2 = mapper.Map<DateTimeOffset>(dtoDynamic);

        dto2.ShouldBe(dto1);
    }

    [Fact]
    public void List_of_int_should_serialize()
    {
        var source = new List<int> { 1, 11 };

        var result = Serialize(source);

        result.ShouldNotBeSameAs(source);
        result.SequenceShouldBeEqual(source);
    }

    [Fact]
    public void List_of_nullable_int_should_serialize()
    {
        var source = new List<int?> { null, 1, 11 };

        var result = Serialize(source);

        result.ShouldNotBeSameAs(source);
        result.SequenceShouldBeEqual(source);
    }

    [Fact]
    public void List_of_nullable_int_should_serialize_as_ienumerable()
    {
        var source = new List<int?> { null, 1, 11 };

        var result = Serialize<IEnumerable<int?>>(source);

        result.ShouldNotBeSameAs(source);
        result.SequenceShouldBeEqual(source);
    }

    [Fact]
    public void Array_of_int_should_serialize()
    {
        var source = new[] { 1, 11 };

        var result = Serialize(source);

        result.ShouldNotBeSameAs(source);
        result.SequenceShouldBeEqual(source);
    }

    [Fact]
    public void Array_of_nullable_int_should_serialize()
    {
        var source = new int?[] { null, 1, 11 };

        var result = Serialize(source);

        result.ShouldNotBeSameAs(source);
        result.SequenceShouldBeEqual(source);
    }

    [Fact]
    public void Three_dimensional_array_of_nullable_int_should_serialize()
    {
#pragma warning disable SA1500 // Braces for multi-line statements should not share line
        var array = new int?[,,]
        {
            {
                { null, 8, 1, 1, 1 }, { 1, 8, 1, 1, 1 }, { 11, 8, 1, 1, 1 },
                { 8, null, 1, 1, 1 }, { 8, 1, 1, 1, 1 }, { 8, 11, 1, 1, 1 },
            },
            {
                { null, 4, 1, 1, 1 }, { 1, 4, 1, 1, 1 }, { 11, 4, 1, 1, 1 },
                { null, 4, 1, 1, 1 }, { 1, 4, 1, 1, 1 }, { 11, 4, 1, 1, 1 },
            },
        };
#pragma warning restore SA1500 // Braces for multi-line statements should not share line

        var result = Serialize<int?[,,], int?[]>(array);
        result.Length.ShouldBe(60);
    }

    [Fact]
    public void Array_of_char_should_serialize()
    {
        var source = new[] { 'h', 'e', 'l', 'l', 'o' };

        var result = Serialize(source);

        result.ShouldNotBeSameAs(source);
        result.SequenceShouldBeEqual(source);
    }

    [Fact]
    public void Nested_anonymous_type_with_nullable_int_property_should_serialize()
    {
        int? i = 1;
        sbyte? b1 = -2;
        byte b2 = 2;
        Should.NotThrow(() => Serialize(new { B = b1, N = new { I = i, B = b2 } }));
    }

    [Fact]
    public void Float_as_string_should_serialize()
    {
        var result = Serialize<float, float>(0.1F, formatValuesAsStrings: true);
        result.ShouldBe(0.1F);
    }

    [Fact]
    public void Custom_reference_type_should_serialize()
    {
        var source = new A<int> { Value = 1 };

        var result = Serialize(source);

        result.Value.ShouldBe(1);
    }

    [Fact]
    public void Custom_reference_type_array_should_serialize()
    {
        var source = new[]
        {
            new A<int> { Value = 1 },
            new A<int> { Value = 2 },
        };

        var result = Serialize(source);

        result.SequenceShouldBeEqual(source, (x, y) => x.Value == y.Value);
    }

    [Fact]
    public void Custom_reference_type_with_property_of_custom_reference_type_array_should_serialize()
    {
        var source = new A<A<int>[]>
        {
            Value =
            [
                new A<int> { Value = 1 },
                new A<int> { Value = 2 },
            ],
        };

        var result = Serialize(source);

        result.Value.SequenceShouldBeEqual(source.Value, (x, y) => x.Value == y.Value);
    }

    [Fact]
    public void List_of_custom_reference_type_with_property_of_custom_reference_type_array_should_serialize()
    {
        var source = new List<A<A<int>[]>>
        {
            new()
            {
                Value =
                [
                    new A<int> { Value = 1 },
                    new A<int> { Value = 2 },
                ],
            },
            new()
            {
                Value =
                [
                    new A<int> { Value = 3 },
                ],
            },
        };

        var result = Serialize(source);

        result.SequenceShouldBeEqual(
            source,
            (x, y) => x.Value.SequenceEqual(
                y.Value,
                (x, y) => x.Value == y.Value));
    }

    private object SerializeAsProperty(Type propertyTape, object propertyValue, bool setTypeFromGenericArgument = true, bool formatValuesAsStrings = false)
    {
        var method = typeof(When_serializing_dynamic_object)
          .GetMethods(PrivateInstance)
          .Single(x => x.Name == nameof(SerializeAsProperty) && x.IsGenericMethod && x.GetGenericArguments().Length is 1);
        return method.MakeGenericMethod(propertyTape).Invoke(this, [propertyValue, setTypeFromGenericArgument, formatValuesAsStrings]);
    }

    private T SerializeAsProperty<T>(T value, bool setTypeFromGenericArgument = true, bool formatValuesAsStrings = false)
        => Serialize<A<T>, A<T>>(new A<T> { Value = value }, setTypeFromGenericArgument, formatValuesAsStrings).Value;

    private object Serialize(Type type, object value, bool setTypeFromGenericArgument = true, bool formatValuesAsStrings = false)
    {
        var method = typeof(When_serializing_dynamic_object)
            .GetMethods(PrivateInstance)
            .Single(x => x.Name == nameof(Serialize) && x.IsGenericMethod && x.GetGenericArguments().Length is 1 && x.GetParameters().Length is 3);
        return method.MakeGenericMethod(type).Invoke(this, [value, setTypeFromGenericArgument, formatValuesAsStrings]);
    }

    private T Serialize<T>(T value, bool setTypeFromGenericArgument, bool formatValuesAsStrings)
        => Serialize<T, T>(value, setTypeFromGenericArgument, formatValuesAsStrings);

    private TResult Serialize<TSource, TResult>(TSource value, bool setTypeFromGenericArgument = true, bool formatValuesAsStrings = false)
    {
        var settings = new DynamicObjectMapperSettings { FormatNativeTypesAsString = formatValuesAsStrings };
        var dynamicObject = new DynamicObjectMapper(settings).MapObject(value);
        if (dynamicObject is not null && setTypeFromGenericArgument)
        {
            dynamicObject.Type = new TypeInfo(typeof(TSource), false, false);
        }

        var serializedDynamicObject = _serialize(dynamicObject);

        var resurectedValue = new DynamicObjectMapper().Map<TResult>(serializedDynamicObject);
        return resurectedValue;
    }

    private T Serialize<T>(T source)
    {
        var dynamicSourceObject = DynamicObject.Create(source);

        var dynamicSourceResult = _serialize(dynamicSourceObject);

        dynamicSourceResult.ShouldNotBeSameAs(dynamicSourceObject);

        var result = dynamicSourceResult.CreateObject();

        result.ShouldNotBeSameAs(source);
        result.ShouldBeOfType(source.GetType());
        return result.ShouldBeAssignableTo<T>();
    }
}