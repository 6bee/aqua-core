// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using Aqua.TypeSystem;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using Xunit;

    public abstract class When_serializing_dynamic_object
    {
        public class JsonSerializer : When_serializing_dynamic_object
        {
            public JsonSerializer() : base(JsonSerializationHelper.Serialize) { }
        }

        public class DataContractSerializer : When_serializing_dynamic_object
        {
            public DataContractSerializer() : base(DataContractSerializationHelper.Serialize) { }
        }

        // XML serialization doesn't support circular references
        //public class XmlSerializer : When_serializing_dynamic_object
        //{
        //    public XmlSerializer() : base(XmlSerializationHelper.Serialize) { }
        //}

#if NET
        public class BinaryFormatter : When_serializing_dynamic_object
        {
            public BinaryFormatter() : base(BinarySerializationHelper.Serialize) { }
        }

        public class NetDataContractSerializer : When_serializing_dynamic_object
        {
            public NetDataContractSerializer() : base(NetDataContractSerializationHelper.Serialize) { }
        }
#endif
        
        readonly Func<DynamicObject, DynamicObject> _serialize;

        protected When_serializing_dynamic_object(Func<DynamicObject, DynamicObject> serialize)
        {
            _serialize = serialize;
        }

        [Fact]
        public void List_of_nullable_int_should_serialize()
        {
            var result = Serialize<IEnumerable<int?>>(new List<int?> { null, 1, 11 });
        }

        [Fact]
        public void Array_of_int_should_serialize()
        {
            var result = Serialize(new int[] { 1, 11 });
        }

        [Fact]
        public void Array_of_nullable_int_should_serialize()
        {
            var result = Serialize(new int?[] { null, 1, 11 });
        }

        [Fact]
        public void Three_dimensional_array_of_nullable_int_should_serialize()
        {
            var array = new int?[,,]
            {
                {
                    { null, 8, 1, 1, 1 }, { 1, 8, 1, 1, 1 }, { 11, 8, 1, 1, 1 },
                    { 8, null, 1, 1, 1 }, { 8, 1, 1, 1, 1 }, { 8, 11, 1, 1, 1 },
                },
                {
                    { null, 4, 1, 1, 1 }, { 1, 4, 1, 1, 1 }, { 11, 4, 1, 1, 1 },
                    { null, 4, 1, 1, 1 }, { 1, 4, 1, 1, 1 }, { 11, 4, 1, 1, 1 },
                }
            };

            var result = Serialize<int?[], int?[,,]>(array);
            result.Length.ShouldBe(60);
        }

        [Fact]
        public void Array_of_char_should_serialize()
        {
            var result = Serialize(new[] { 'h', 'e', 'l', 'l', 'o' });
        }

        [Fact]
        public void Anonymous_type_with_nullable_int_property_should_serialize()
        {
            int? i = 1;
            var result = Serialize(new { I = i });
        }

        [Fact]
        public void Nested_anonymous_type_with_nullable_int_property_should_serialize()
        {
            int? i = 1;
            sbyte? b1 = -2;
            byte b2 = 2;

            var result = Serialize(new { B = b1, N = new { I = i, B = b2 } });
        }

        private T Serialize<T>(T value, bool setTypeFromGenericArgument = true)
            => Serialize<T, T>(value, setTypeFromGenericArgument);

        private TResult Serialize<TResult,TSource>(TSource value, bool setTypeFromGenericArgument = true)
        {
            var dynamicObject = DynamicObject.Create(value);
            if (setTypeFromGenericArgument)
            {
                dynamicObject.Type = new TypeInfo(typeof(TSource));
            }

            var serializedDynamicObject = _serialize(dynamicObject);

            var resurectedValue = new DynamicObjectMapper().Map<TResult>(serializedDynamicObject);
            return resurectedValue;
        }
    }
}
