﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.TypeSystem.TypeInfo
{
    using Aqua.Dynamic;
    using Aqua.TypeSystem;
    using Shouldly;
    using System;
    using Xunit;

    public abstract partial class When_serializing_typeinfo_of_dynamicobject
    {
#pragma warning disable SA1128 // Put constructor initializers on their own line
#pragma warning disable SA1502 // Element should not be on a single line

        public class BinaryFormatter : When_serializing_typeinfo_of_dynamicobject
        {
            public BinaryFormatter() : base(BinarySerializationHelper.Serialize) { }
        }

        public class DataContractSerializer : When_serializing_typeinfo_of_dynamicobject
        {
            public DataContractSerializer() : base(DataContractSerializationHelper.Serialize) { }
        }

        public class JsonSerializer : When_serializing_typeinfo_of_dynamicobject
        {
            public JsonSerializer() : base(JsonSerializationHelper.Serialize) { }
        }

        // XmlSerializer doesn't support circular references
        // protobuf-net doesn't support circular references
#if NETFX
        public class NetDataContractSerializer : When_serializing_typeinfo_of_dynamicobject
        {
            public NetDataContractSerializer() : base(NetDataContractSerializationHelper.Serialize) { }
        }
#endif // NETFX

#pragma warning restore SA1502 // Element should not be on a single line
#pragma warning restore SA1128 // Put constructor initializers on their own line

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
