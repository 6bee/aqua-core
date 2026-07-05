// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.TypeSystem.TypeInfo;

using Aqua.TypeSystem;

public abstract class When_using_typeinfo_with_circular_reference_no_propertyinfos
{
    public class With_data_contract_serializer() : When_using_typeinfo_with_circular_reference_no_propertyinfos(DataContractSerializationHelper.Clone);

    public class With_newtown_json_serializer() : When_using_typeinfo_with_circular_reference_no_propertyinfos(NewtonsoftJsonSerializationHelper.Clone);

    public class With_system_text_json_serializer() : When_using_typeinfo_with_circular_reference_no_propertyinfos(SystemTextJsonSerializationHelper.Clone);

    public class With_messagepack_serializer() : When_using_typeinfo_with_circular_reference_no_propertyinfos(MessagePackSerializationHelper.Clone);

    public class With_protobuf_serializer() : When_using_typeinfo_with_circular_reference_no_propertyinfos(ProtobufSerializationHelper.Clone);

    public class With_xml_serializer() : When_using_typeinfo_with_circular_reference_no_propertyinfos(XmlSerializationHelper.Serialize);

#if NETFRAMEWORK
    public class With_binary_formatter() : When_using_typeinfo_with_circular_reference_no_propertyinfos(BinarySerializationHelper.Clone);

    public class With_net_data_contract_serializer() : When_using_typeinfo_with_circular_reference_no_propertyinfos(NetDataContractSerializationHelper.Clone);
#endif // NETFRAMEWORK

    private abstract class A
    {
        public int Number { get; set; }
    }

    private class C<T> : A
    {
        public T Reference { get; set; }
    }

    private class X
    {
    }

    private readonly TypeInfo serializedTypeInfo;

    protected When_using_typeinfo_with_circular_reference_no_propertyinfos(Func<TypeInfo, TypeInfo> serialize)
    {
        var typeInfo = new TypeInfo(typeof(C<X>), false);

        serializedTypeInfo = serialize(typeInfo);
    }

    [Fact]
    public void Type_info_should_have_typename()
    {
        serializedTypeInfo.Name.ShouldBe("C`1");
    }

    [Fact]
    public void Type_info_should_be_generic()
    {
        serializedTypeInfo.IsGenericType.ShouldBeTrue();
    }
}
