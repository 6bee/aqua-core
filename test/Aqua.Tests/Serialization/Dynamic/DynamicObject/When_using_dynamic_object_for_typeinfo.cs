// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.Dynamic.DynamicObject;

using Aqua.Dynamic;
using Aqua.TypeSystem;
using Shouldly;
using System;
using Xunit;

public abstract class When_using_dynamic_object_for_typeinfo(Func<DynamicObject, DynamicObject> serialize)
{
#if !NET8_0_OR_GREATER
    public class With_binary_formatter() : When_using_dynamic_object_for_typeinfo(BinarySerializationHelper.Clone);

#endif // NET8_0_OR_GREATER

    public class With_data_contract_serializer() : When_using_dynamic_object_for_typeinfo(DataContractSerializationHelper.Clone);

    public class With_newtown_json_serializer() : When_using_dynamic_object_for_typeinfo(NewtonsoftJsonSerializationHelper.Clone);

    public class With_system_text_json_serializer() : When_using_dynamic_object_for_typeinfo(SystemTextJsonSerializationHelper.Clone);

#if NETFRAMEWORK
    public class With_net_data_contract_serializer() : When_using_dynamic_object_for_typeinfo(NetDataContractSerializationHelper.Clone);
#endif // NETFRAMEWORK

    public class With_protobuf_net_serializer() : When_using_dynamic_object_for_typeinfo(ProtobufNetSerializationHelper.Clone);

    public class With_xml_serializer() : When_using_dynamic_object_for_typeinfo(XmlSerializationHelper.Serialize);

    [Theory]
    [MemberData(nameof(TestData.TestTypes), MemberType = typeof(TestData))]
    public void Should_map_type_to_dynamic_object_and_back(Type type)
    {
        var settings = new DynamicObjectMapperSettings { PassthroughAquaTypeSystemTypes = false };
        var dynamicObject = new DynamicObjectMapper(settings).MapObject(type);
        var serialized = serialize(dynamicObject);
        var resurectedType = (Type)new DynamicObjectMapper().Map(serialized);

        dynamicObject.Type.ToType().ShouldBe(typeof(Type));
        dynamicObject[nameof(TypeInfo.Name)].ShouldBe(type.Name);
        dynamicObject[nameof(TypeInfo.Namespace)].ShouldBe(type.Namespace);

        resurectedType.ShouldBe(type);
    }
}