// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.TypeSystem.TypeInfo
{
    using Aqua.TypeSystem;
    using Shouldly;
    using System;
    using System.Linq;
    using Xunit;

    public abstract class When_using_typeinfo_with_circular_reference
    {
        // XmlSerializer doesn't support circular references
        // protobuf-net doesn't support circular references
        public class With_binary_formatter : When_using_typeinfo_with_circular_reference
        {
            public With_binary_formatter()
                : base(BinarySerializationHelper.Serialize)
            {
            }
        }

        public class With_data_contract_serializer : When_using_typeinfo_with_circular_reference
        {
            public With_data_contract_serializer()
                : base(DataContractSerializationHelper.Serialize)
            {
            }
        }

        public class With_newtown_json_serializer : When_using_typeinfo_with_circular_reference
        {
            public With_newtown_json_serializer()
                : base(NewtonsoftJsonSerializationHelper.Serialize)
            {
            }
        }

        public class With_system_text_json_serializer : When_using_typeinfo_with_circular_reference
        {
            public With_system_text_json_serializer()
                : base(SystemTextJsonSerializationHelper.Serialize)
            {
            }
        }

        public class With_json_serializer : When_using_typeinfo_with_circular_reference
        {
            public With_json_serializer()
                : base(SystemTextJsonSerializationHelper.Serialize)
            {
            }
        }

#if NETFRAMEWORK
        public class With_net_data_contract_serializer : When_using_typeinfo_with_circular_reference
        {
            public With_net_data_contract_serializer()
                : base(NetDataContractSerializationHelper.Serialize)
            {
            }
        }
#endif // NETFRAMEWORK

        private class A
        {
            public A SelfReference { get; set; }
        }

        private readonly TypeInfo serializedTypeInfo;

        protected When_using_typeinfo_with_circular_reference(Func<TypeInfo, TypeInfo> serialize)
        {
            var typeInfo = new TypeInfo(typeof(A), true);

            serializedTypeInfo = serialize(typeInfo);

            serializedTypeInfo.ShouldNotBeSameAs(typeInfo);
        }

        [Fact]
        public void Serialization_should_leave_circular_reference_intact()
        {
            serializedTypeInfo.Properties.Single().DeclaringType.ShouldBeSameAs(serializedTypeInfo);
        }
    }
}
