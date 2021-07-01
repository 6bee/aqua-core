// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.TypeSystem.TypeInfo
{
    using Aqua.Dynamic;
    using Aqua.TypeSystem;
    using Shouldly;
    using System;
    using Xunit;

    public abstract class When_serializing_typeinfo_of_dynamicobject
    {
        // XmlSerializer doesn't support circular references
        // protobuf-net doesn't support circular references
        public class With_binary_formatter : When_serializing_typeinfo_of_dynamicobject
        {
            public With_binary_formatter()
                : base(BinarySerializationHelper.Serialize)
            {
            }
        }

        public class With_data_contract_serializer : When_serializing_typeinfo_of_dynamicobject
        {
            public With_data_contract_serializer()
                : base(DataContractSerializationHelper.Serialize)
            {
            }
        }

        public class With_json_serializer : When_serializing_typeinfo_of_dynamicobject
        {
            public With_json_serializer()
                : base(JsonSerializationHelper.Serialize)
            {
            }
        }

#if NETFRAMEWORK
        public class With_net_data_contract_serializer : When_serializing_typeinfo_of_dynamicobject
        {
            public With_net_data_contract_serializer()
                : base(NetDataContractSerializationHelper.Serialize)
            {
            }
        }
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
}
