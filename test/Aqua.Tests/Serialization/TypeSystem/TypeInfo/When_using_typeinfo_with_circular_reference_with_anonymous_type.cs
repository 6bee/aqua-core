﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.TypeSystem.TypeInfo
{
    using Aqua.TypeSystem;
    using Shouldly;
    using System;
    using Xunit;

    public abstract partial class When_using_typeinfo_with_circular_reference_with_anonymous_type
    {
#pragma warning disable SA1128 // Put constructor initializers on their own line
#pragma warning disable SA1502 // Element should not be on a single line

        public class BinaryFormatter : When_using_typeinfo_with_circular_reference_with_anonymous_type
        {
            public BinaryFormatter() : base(BinarySerializationHelper.Serialize) { }
        }

        public class DataContractSerializer : When_using_typeinfo_with_circular_reference_with_anonymous_type
        {
            public DataContractSerializer() : base(DataContractSerializationHelper.Serialize) { }
        }

        public class JsonSerializer : When_using_typeinfo_with_circular_reference_with_anonymous_type
        {
            public JsonSerializer() : base(JsonSerializationHelper.Serialize) { }
        }

#if NETFX
        public class NetDataContractSerializer : When_using_typeinfo_with_circular_reference_with_anonymous_type
        {
            public NetDataContractSerializer() : base(NetDataContractSerializationHelper.Serialize) { }
        }
#endif // NETFX

#if COREFX
        public class ProtobufNetSerializer : When_using_typeinfo_with_circular_reference_with_anonymous_type
        {
            public ProtobufNetSerializer() : base(ProtobufNetSerializationHelper.Serialize) { }
        }
#endif // COREFX

        public class XmlSerializer : When_using_typeinfo_with_circular_reference_with_anonymous_type
        {
            public XmlSerializer() : base(XmlSerializationHelper.Serialize) { }
        }

#pragma warning restore SA1502 // Element should not be on a single line
#pragma warning restore SA1128 // Put constructor initializers on their own line

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
