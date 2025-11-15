// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.Dynamic.DynamicObject;

using Aqua.Dynamic;
using Aqua.TypeSystem;
using Shouldly;
using System;
using Xunit;

public abstract class When_serializing_dynamicobject_for_typeinfo_of_typeinfo
{
    // XML serialization doesn't support circular references
    // protobuf-net doesn't support circular references
#if !NET8_0_OR_GREATER
    public class With_binary_formatter() : When_serializing_dynamicobject_for_typeinfo_of_typeinfo(BinarySerializationHelper.Clone);
#endif // NET8_0_OR_GREATER

    public class With_data_contract_serializer() : When_serializing_dynamicobject_for_typeinfo_of_typeinfo(DataContractSerializationHelper.Clone);

    public class With_newtown_json_serializer() : When_serializing_dynamicobject_for_typeinfo_of_typeinfo(NewtonsoftJsonSerializationHelper.Clone);

    public class With_system_text_json_serializer() : When_serializing_dynamicobject_for_typeinfo_of_typeinfo(SystemTextJsonSerializationHelper.Clone);

#if NETFRAMEWORK
    public class With_net_data_contract_serializer() : When_serializing_dynamicobject_for_typeinfo_of_typeinfo(NetDataContractSerializationHelper.Clone);
#endif // NETFRAMEWORK

    private readonly DynamicObject dynamicObject;
    private readonly DynamicObject serializedDynamicObject;

    protected When_serializing_dynamicobject_for_typeinfo_of_typeinfo(Func<DynamicObject, DynamicObject> serialize)
    {
        dynamicObject = DynamicObject.Create(new TypeInfo(typeof(TypeInfo), true));
        serializedDynamicObject = serialize(dynamicObject);
    }

    [Fact]
    public void Serialization_should_return_new_instance()
    {
        serializedDynamicObject.ShouldNotBeSameAs(dynamicObject);
    }
}