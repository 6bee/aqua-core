// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.TypeSystem.TypeInfo;

using Aqua.Dynamic;
using Aqua.TypeSystem;
using Shouldly;
using Xunit;

public abstract class When_serializing_typeinfo_of_dynamicobject
{
    // XmlSerializer doesn't support circular references
    // protobuf-net doesn't support circular references
#if !NET8_0_OR_GREATER
    public class With_binary_formatter() : When_serializing_typeinfo_of_dynamicobject(BinarySerializationHelper.Clone);
#endif // NET8_0_OR_GREATER

    public class With_data_contract_serializer() : When_serializing_typeinfo_of_dynamicobject(DataContractSerializationHelper.Clone);

    public class With_newtown_json_serializer() : When_serializing_typeinfo_of_dynamicobject(NewtonsoftJsonSerializationHelper.Clone);

    public class With_system_text_json_serializer() : When_serializing_typeinfo_of_dynamicobject(SystemTextJsonSerializationHelper.Clone);

#if NETFRAMEWORK
    public class With_net_data_contract_serializer() : When_serializing_typeinfo_of_dynamicobject(NetDataContractSerializationHelper.Clone);
#endif // NETFRAMEWORK

    private readonly TypeInfo typeInfo;
    private readonly TypeInfo serializedTypeInfo;

    protected When_serializing_typeinfo_of_dynamicobject(Func<TypeInfo, TypeInfo> serialize)
    {
        typeInfo = new TypeInfo(typeof(DynamicObject), true);
        serializedTypeInfo = serialize(typeInfo);
    }

    [Fact]
    public void Serialization_should_return_new_instance()
    {
        serializedTypeInfo.ShouldNotBeSameAs(typeInfo);
    }
}