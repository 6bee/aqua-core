// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.TypeSystem.TypeInfo
{
    using Aqua.TypeSystem;
    using Shouldly;
    using System;
    using Xunit;

    public abstract class When_using_typeinfo_with_circular_reference_no_propertyinfos
    {
        public class With_binary_formatter : When_using_typeinfo_with_circular_reference_no_propertyinfos
        {
            public With_binary_formatter()
                : base(BinarySerializationHelper.Serialize)
            {
            }
        }

        public class With_data_contract_serializer : When_using_typeinfo_with_circular_reference_no_propertyinfos
        {
            public With_data_contract_serializer()
                : base(DataContractSerializationHelper.Serialize)
            {
            }
        }

        public class With_newtown_json_serializer : When_using_typeinfo_with_circular_reference_no_propertyinfos
        {
            public With_newtown_json_serializer()
                : base(NewtonsoftJsonSerializationHelper.Serialize)
            {
            }
        }

        public class With_system_text_json_serializer : When_using_typeinfo_with_circular_reference_no_propertyinfos
        {
            public With_system_text_json_serializer()
                : base(SystemTextJsonSerializationHelper.Serialize)
            {
            }
        }

#if NETFRAMEWORK
        public class With_net_data_contract_serializer : When_using_typeinfo_with_circular_reference_no_propertyinfos
        {
            public With_net_data_contract_serializer()
                : base(NetDataContractSerializationHelper.Serialize)
            {
            }
        }
#endif // NETFRAMEWORK

        public class With_protobuf_net_serializer : When_using_typeinfo_with_circular_reference_no_propertyinfos
        {
            public With_protobuf_net_serializer()
                : base(ProtobufNetSerializationHelper.Serialize)
            {
            }
        }

        public class With_xml_serializer : When_using_typeinfo_with_circular_reference_no_propertyinfos
        {
            public With_xml_serializer()
                : base(XmlSerializationHelper.Serialize)
            {
            }
        }

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
}
