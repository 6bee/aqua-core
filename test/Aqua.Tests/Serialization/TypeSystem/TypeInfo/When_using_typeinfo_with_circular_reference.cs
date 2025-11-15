// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.TypeSystem.TypeInfo;

using Aqua.TypeSystem;
using Shouldly;
using Xunit;

public abstract class When_using_typeinfo_with_circular_reference(Func<TypeInfo, TypeInfo> serialize)
{
    // XmlSerializer doesn't support circular references
    // protobuf-net doesn't support circular references
#if !NET8_0_OR_GREATER
    public class With_binary_formatter() : When_using_typeinfo_with_circular_reference(BinarySerializationHelper.Clone);
#endif // NET8_0_OR_GREATER

    public class With_data_contract_serializer() : When_using_typeinfo_with_circular_reference(DataContractSerializationHelper.Clone);

    public class With_newtown_json_serializer() : When_using_typeinfo_with_circular_reference(NewtonsoftJsonSerializationHelper.Clone);

    public class With_system_text_json_serializer() : When_using_typeinfo_with_circular_reference(SystemTextJsonSerializationHelper.Clone);

#if NETFRAMEWORK
    public class With_net_data_contract_serializer() : When_using_typeinfo_with_circular_reference(NetDataContractSerializationHelper.Clone);
#endif // NETFRAMEWORK

    private class A
    {
        public A SelfReference { get; set; }
    }

    [Fact]
    public void Serialization_should_leave_circular_reference_intact()
    {
        var typeInfo = new TypeInfo(typeof(A), true);

        var serializedTypeInfo = serialize(typeInfo);

        serializedTypeInfo.ShouldNotBeSameAs(typeInfo);

        serializedTypeInfo.Properties.Single().DeclaringType.ShouldBeSameAs(serializedTypeInfo);
    }
}