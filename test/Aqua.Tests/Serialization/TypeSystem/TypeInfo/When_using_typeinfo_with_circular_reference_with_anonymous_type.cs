// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.TypeSystem.TypeInfo;

using Aqua.TypeSystem;
using Shouldly;
using System;
using Xunit;

public abstract class When_using_typeinfo_with_circular_reference_with_anonymous_type
{
#if !NET8_0_OR_GREATER
    public class With_binary_formatter() : When_using_typeinfo_with_circular_reference_with_anonymous_type(BinarySerializationHelper.Clone);
#endif // NET8_0_OR_GREATER

    public class With_data_contract_serializer() : When_using_typeinfo_with_circular_reference_with_anonymous_type(DataContractSerializationHelper.Clone);

    public class With_newtown_json_serializer() : When_using_typeinfo_with_circular_reference_with_anonymous_type(NewtonsoftJsonSerializationHelper.Clone);

    public class With_system_text_json_serializer() : When_using_typeinfo_with_circular_reference_with_anonymous_type(SystemTextJsonSerializationHelper.Clone);

#if NETFRAMEWORK
    public class With_net_data_contract_serializer() : When_using_typeinfo_with_circular_reference_with_anonymous_type(NetDataContractSerializationHelper.Clone);
#endif // NETFRAMEWORK

    public class With_protobuf_net_serializer() : When_using_typeinfo_with_circular_reference_with_anonymous_type(ProtobufNetSerializationHelper.Clone);

    public class With_xml_serializer() : When_using_typeinfo_with_circular_reference_with_anonymous_type(XmlSerializationHelper.Serialize);

    private readonly TypeInfo serializedTypeInfo;

    protected When_using_typeinfo_with_circular_reference_with_anonymous_type(Func<TypeInfo, TypeInfo> serialize)
    {
        var instance = new
        {
            Number = 1,
            Value = new
            {
                X = new { Name = string.Empty },
            },
        };

        var typeInfo = new TypeInfo(instance.GetType(), false, false);

        serializedTypeInfo = serialize(typeInfo);
    }

    [Fact]
    public void Type_info_should_be_generic()
    {
        serializedTypeInfo.IsGenericType.ShouldBeTrue();
    }
}