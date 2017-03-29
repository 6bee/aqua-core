// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using Aqua.TypeSystem.Extensions;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;
    using Xunit;
    using TypeInfo = Aqua.TypeSystem.TypeInfo;
    using TypeResolver = Aqua.TypeSystem.TypeResolver;

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

        // NOTE: XML serialization doesn't support circular references
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

        class A<T>
        {
            public T Value { get; set; }
        }

        public static IEnumerable<object[]> TestData => new object[]
            {
                $"Test values treated as native types in {nameof(DynamicObjectMapper)}",
                byte.MinValue,
                byte.MaxValue,
                sbyte.MinValue,
                sbyte.MaxValue,
                short.MinValue,
                short.MaxValue,
                ushort.MinValue,
                ushort.MaxValue,
                int.MinValue,
                int.MaxValue,
                uint.MinValue,
                uint.MaxValue,
                long.MinValue,
                long.MaxValue,
#if NET // Newtonsoft.Json.JsonReaderException : JSON integer 18446744073709551615 is too large or small for an Int64.
                ulong.MinValue,
                ulong.MaxValue,
#endif
                float.MinValue,
                float.MaxValue,
                double.MinValue,
                double.MaxValue,
                //decimal.MinValue, // json.net screws-up decimal when serialized as object property
                //decimal.MaxValue, // json.net screws-up decimal when serialized as object property
                new decimal(Math.E),
                new decimal(Math.PI),
                char.MinValue,
                char.MaxValue,
                'à',
                true,
                false,
                new Guid(),
                default(Guid),
                Guid.NewGuid(),
                DateTime.Now,
                new TimeSpan(),
                default(TimeSpan),
                new TimeSpan(long.MaxValue),
                // NOTE: DateTimeOffset doesn't work with json.net (when assigned to an object property) since DateTimeOffset gets confused with DateTime
                //default(DateTimeOffset),
                //DateTimeOffset.MinValue,
                //DateTimeOffset.MaxValue,
                //new DateTimeOffset(new DateTime(2012, 12, 12), new TimeSpan(12, 12, 0)),
                // NOTE: BigInteger doesn't work with json.net (when assigned to an object property) since values seem to get lost (on .net core only)
                //new BigInteger(),
                //default(BigInteger),
                //new BigInteger(ulong.MinValue) - 1,
                //new BigInteger(ulong.MaxValue) + 1,
                // NOTE: Complex doesn't work with json.net (when assigned to an object property) since values seem to get lost entirely
                //new Complex(),
                //default(Complex),
                //new Complex(32, -87654),
                //new Complex(-87654, 234),
                //new Complex(double.MinValue, double.MinValue),
                //new Complex(double.MaxValue, double.MaxValue),
            }
            .SelectMany(x => new Tuple<Type, object>[]
            {
                Tuple.Create(x.GetType(), x),
                Tuple.Create(x.GetType().IsClass() ? x.GetType() : typeof(Nullable<>).MakeGenericType(x.GetType()), x),
                Tuple.Create(x.GetType().MakeArrayType(), CreateArray(x)),
                // NOTE: doesn't work with json.net since list element types don't get corrected by PrimitiveValueInspector
                //Tuple.Create(typeof(List<>).MakeGenericType(x.GetType()), CreateList(x)),
            })
            .Distinct()
            .Select(x => new object[] { x.Item1, x.Item2 });

        private static object CreateArray(object item)
        {
            var type = item.GetType();
            var toArrayMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray), BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(type);
            return toArrayMethod.Invoke(null, new[] { CreateEnumerable(item) });
        }

        private static object CreateList(object item)
        {
            var type = item.GetType();
            var toListMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList), BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(type);
            return toListMethod.Invoke(null, new[] { CreateEnumerable(item) });
        }

        private static object CreateEnumerable(object item)
        {
            var type = item.GetType();
            var array = new[] { item, item };
            var castMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast), BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(type);
            return castMethod.Invoke(null, new[] { array });
        }

        private static readonly MethodInfo SerializeMethod = typeof(When_serializing_dynamic_object)
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(x => x.Name == nameof(Serialize) && x.GetGenericArguments().Length == 1);

        private static readonly MethodInfo SerializeAsPropertyMethod = typeof(When_serializing_dynamic_object)
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(x => x.Name == nameof(SerializeAsProperty) && x.GetGenericArguments().Length == 1);

        private readonly Func<DynamicObject, DynamicObject> _serialize;

        protected When_serializing_dynamic_object(Func<DynamicObject, DynamicObject> serialize)
        {
            _serialize = serialize;
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void Should_serialize_value(Type type, object value)
        {
            var result = SerializeMethod.MakeGenericMethod(type).Invoke(this, new[] { value, true });
            result.ShouldBe(value, $"type: {type.FullName}");
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void Should_serialize_value_as_property(Type type, object value)
        {
            var result = SerializeAsPropertyMethod.MakeGenericMethod(type).Invoke(this, new[] { value, true, false });
            result.ShouldBe(value, $"type: {type.FullName}");
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void Should_serialize_value_as_property_when_using_string_formatting(Type type, object value)
        {
            var result = SerializeAsPropertyMethod.MakeGenericMethod(type).Invoke(this, new[] { value, true, true });
            result.ShouldBe(value, $"type: {type.FullName}");
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
        public void Nested_anonymous_type_with_nullable_int_property_should_serialize()
        {
            int? i = 1;
            sbyte? b1 = -2;
            byte b2 = 2;

            var result = Serialize(new { B = b1, N = new { I = i, B = b2 } });
        }

        private T Serialize<T>(T value, bool setTypeFromGenericArgument = true)
            => Serialize<T, T>(value, setTypeFromGenericArgument);

        private T SerializeAsProperty<T>(T value, bool setTypeFromGenericArgument = true, bool formatValuesAsStrings = false)
            => Serialize<A<T>, A<T>>(new A<T> { Value = value }, setTypeFromGenericArgument).Value;

        private TResult Serialize<TResult, TSource>(TSource value, bool setTypeFromGenericArgument = true, bool formatValuesAsStrings = false)
        {
            var settings = new DynamicObjectMapperSettings { FormatPrimitiveTypesAsString = formatValuesAsStrings };
            var dynamicObject = new DynamicObjectMapper(settings).MapObject(value);
            if (setTypeFromGenericArgument)
            {
                dynamicObject.Type = new TypeInfo(typeof(TSource), false);
            }

            var serializedDynamicObject = _serialize(dynamicObject);
            
            var resurectedValue = new DynamicObjectMapper().Map<TResult>(serializedDynamicObject);
            return resurectedValue;
        }
    }
}
