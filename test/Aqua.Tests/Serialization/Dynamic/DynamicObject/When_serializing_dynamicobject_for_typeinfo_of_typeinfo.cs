// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.DynamicObject
{
    using Aqua.Dynamic;
    using Aqua.TypeSystem;
    using Shouldly;
    using System;
    using Xunit;

    public abstract class When_serializing_dynamicobject_for_typeinfo_of_typeinfo
    {
#if NET
        // TODO: fails on netcoreapp1.0 - not sure why!?
        // Error: The active Test Run was aborted.
        public class DataContractSerializer : When_serializing_dynamicobject_for_typeinfo_of_typeinfo
        {
            public DataContractSerializer() : base(DataContractSerializationHelper.Serialize) { }
        }
#endif

#if NET
        // TODO: fails on netcoreapp1.0 - not sure why!?
        // Error: The active Test Run was aborted.
        public class JsonSerializer : When_serializing_dynamicobject_for_typeinfo_of_typeinfo
        {
            public JsonSerializer() : base(JsonSerializationHelper.Serialize) { }
        }
#endif
        
        // XML serialization doesn't support circular references

#if NET
        public class BinaryFormatter : When_serializing_dynamicobject_for_typeinfo_of_typeinfo
        {
            public BinaryFormatter() : base(BinarySerializationHelper.Serialize) { }
        }
#endif

#if NET && !NETCOREAPP2
        public class NetDataContractSerializer : When_serializing_dynamicobject_for_typeinfo_of_typeinfo
        {
            public NetDataContractSerializer() : base(NetDataContractSerializationHelper.Serialize) { }
        }
#endif

        DynamicObject dynamicObject;
        DynamicObject serializedDynamicObject;

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
