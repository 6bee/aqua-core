// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.Dynamic.DynamicObject;

using Aqua.Dynamic;
using Shouldly;
using Xunit;

public abstract class When_using_dynamic_object_for_complex_object_tree
{
#if !NET8_0_OR_GREATER
    public class With_binary_formatter() : When_using_dynamic_object_for_complex_object_tree(BinarySerializationHelper.Clone);

#endif // NET8_0_OR_GREATER

    public class With_data_contract_serializer() : When_using_dynamic_object_for_complex_object_tree(DataContractSerializationHelper.Clone);

    public class With_newtown_json_serializer() : When_using_dynamic_object_for_complex_object_tree(NewtonsoftJsonSerializationHelper.Clone);

    public class With_system_text_json_serializer() : When_using_dynamic_object_for_complex_object_tree(SystemTextJsonSerializationHelper.Clone);

#if NETFRAMEWORK
    public class With_net_data_contract_serializer() : When_using_dynamic_object_for_complex_object_tree(NetDataContractSerializationHelper.Clone);
#endif // NETFRAMEWORK

    public class With_protobuf_net_serializer() : When_using_dynamic_object_for_complex_object_tree(ProtobufNetSerializationHelper.Clone);

    public class With_xml_serializer() : When_using_dynamic_object_for_complex_object_tree(XmlSerializationHelper.Serialize);

    private const double DoubleValue = 1.2345679e-87;
    private const string StringValue = "eleven";
    private const string CustomType = "system-string-type";

    private readonly DynamicObject serializedObject;

    protected When_using_dynamic_object_for_complex_object_tree(Func<DynamicObject, DynamicObject> serialize)
    {
        var originalObject = new DynamicObject
        {
            Properties = new PropertySet
            {
                { "DoubleValue", DoubleValue },
                {
                    "Reference", new DynamicObject(typeof(string))
                    {
                        Properties = new PropertySet
                        {
                            { "StringValue", StringValue },
                            { "Type", CustomType },
                        },
                    }
                },
            },
        };

        serializedObject = serialize(originalObject);
    }

    [Fact]
    public void Clone_should_contain_simple_numeric_property()
    {
        serializedObject["DoubleValue"].ShouldBe(DoubleValue);
    }

    [Fact]
    public void Clone_should_contain_nested_string_property()
    {
        var nestedObject = serializedObject["Reference"] as DynamicObject;

        nestedObject["StringValue"].ShouldBe(StringValue);
    }

    [Fact]
    public void Clone_should_contain_nested_type_property()
    {
        var nestedObject = serializedObject["Reference"].ShouldBeOfType<DynamicObject>();

        nestedObject["Type"].ShouldBe(CustomType);
    }

    [Fact]
    public void Clone_should_contain_type_information()
    {
        var nestedObject = serializedObject["Reference"].ShouldBeOfType<DynamicObject>();

        var typeInfo = nestedObject.Type.ShouldNotBeNull();

        typeInfo.ToType().ShouldBe(typeof(string));
    }
}