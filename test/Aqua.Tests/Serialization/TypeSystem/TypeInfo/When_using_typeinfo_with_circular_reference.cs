// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.TypeSystem.TypeInfo
{
    using Aqua.TypeSystem;
    using Shouldly;
    using System;
    using System.Linq;
    using Xunit;

    public abstract partial class When_using_typeinfo_with_circular_reference
    {
        public class BinaryFormatter : When_using_typeinfo_with_circular_reference
        {
            public BinaryFormatter()
                : base(BinarySerializationHelper.Serialize)
            {
            }
        }

        public class DataContractSerializer : When_using_typeinfo_with_circular_reference
        {
            public DataContractSerializer()
                : base(DataContractSerializationHelper.Serialize)
            {
            }
        }

        public class JsonSerializer : When_using_typeinfo_with_circular_reference
        {
            public JsonSerializer()
                : base(JsonSerializationHelper.Serialize)
            {
            }
        }

        // XmlSerializer doesn't support circular references
        // protobuf-net doesn't support circular references
#if NETFRAMEWORK
        public class NetDataContractSerializer : When_using_typeinfo_with_circular_reference
        {
            public NetDataContractSerializer()
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
