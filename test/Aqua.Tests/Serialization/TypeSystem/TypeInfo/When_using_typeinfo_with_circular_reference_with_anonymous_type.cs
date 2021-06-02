// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.TypeSystem.TypeInfo
{
    using Aqua.TypeSystem;
    using Shouldly;
    using System;
    using Xunit;

    public abstract partial class When_using_typeinfo_with_circular_reference_with_anonymous_type
    {
        public class With_binary_formatter : When_using_typeinfo_with_circular_reference_with_anonymous_type
        {
            public With_binary_formatter()
                : base(BinarySerializationHelper.Serialize)
            {
            }
        }

        public class With_data_contract_serializer : When_using_typeinfo_with_circular_reference_with_anonymous_type
        {
            public With_data_contract_serializer()
                : base(DataContractSerializationHelper.Serialize)
            {
            }
        }

        public class With_json_serializer : When_using_typeinfo_with_circular_reference_with_anonymous_type
        {
            public With_json_serializer()
                : base(JsonSerializationHelper.Serialize)
            {
            }
        }

#if NETFRAMEWORK
        public class With_net_data_contract_serializer : When_using_typeinfo_with_circular_reference_with_anonymous_type
        {
            public With_net_data_contract_serializer()
                : base(NetDataContractSerializationHelper.Serialize)
            {
            }
        }
#endif // NETFRAMEWORK

        public class With_protobuf_net_serializer : When_using_typeinfo_with_circular_reference_with_anonymous_type
        {
            public With_protobuf_net_serializer()
                : base(ProtobufNetSerializationHelper.Serialize)
            {
            }
        }

        public class With_xml_serializer : When_using_typeinfo_with_circular_reference_with_anonymous_type
        {
            public With_xml_serializer()
                : base(XmlSerializationHelper.Serialize)
            {
            }
        }

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
}
