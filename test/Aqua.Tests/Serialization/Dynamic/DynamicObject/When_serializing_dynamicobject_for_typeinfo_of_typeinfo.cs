﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using Aqua.TypeSystem;
    using Shouldly;
    using System;
    using Xunit;

    public abstract class When_serializing_dynamicobject_for_typeinfo_of_typeinfo
    {
        // XML serialization doesn't support circular references
        // protobuf-net doesn't support circular references
        public class With_binary_formatter : When_serializing_dynamicobject_for_typeinfo_of_typeinfo
        {
            public With_binary_formatter()
                : base(BinarySerializationHelper.Serialize)
            {
            }
        }

        public class With_data_contract_serializer : When_serializing_dynamicobject_for_typeinfo_of_typeinfo
        {
            public With_data_contract_serializer()
                : base(DataContractSerializationHelper.Serialize)
            {
            }
        }

        public class With_json_serializer : When_serializing_dynamicobject_for_typeinfo_of_typeinfo
        {
            public With_json_serializer()
                : base(JsonSerializationHelper.Serialize)
            {
            }
        }

#if NETFRAMEWORK
        public class With_net_data_contract_serializer : When_serializing_dynamicobject_for_typeinfo_of_typeinfo
        {
            public With_net_data_contract_serializer()
                : base(NetDataContractSerializationHelper.Serialize)
            {
            }
        }
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
}
