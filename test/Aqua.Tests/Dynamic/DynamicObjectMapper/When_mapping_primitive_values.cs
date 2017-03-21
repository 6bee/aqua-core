// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper
{
    using Aqua.Dynamic;
    using Aqua.TypeSystem.Extensions;
    using Shouldly;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Reflection;
    using Xunit;

    public class When_mapping_primitive_values
    {
        class A<T>
        {
            public T Value { get; set; }
        }
        
        public static IEnumerable<object[]> TestData => new object[]
            {
                $"Test values a treated as native types in {nameof(DynamicObjectMapper)}",
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
                ulong.MinValue,
                ulong.MaxValue,
                float.MinValue,
                float.MaxValue,
                double.MinValue,
                double.MaxValue,
                decimal.MinValue,
                decimal.MaxValue,
                char.MinValue,
                char.MaxValue,
                'à',
                true,
                false,
                Guid.NewGuid(),
                DateTime.Now,
                new TimeSpan(),
                new DateTimeOffset(),
#if NET || NETSTANDARD  || CORECLR
                new System.Numerics.BigInteger(),
                new System.Numerics.Complex(),
#endif
            }
            .SelectMany(x => new Tuple<Type, object>[]
            {
                Tuple.Create(x.GetType(), x),
                Tuple.Create(x.GetType().IsClass() ? x.GetType() : typeof(Nullable<>).MakeGenericType(x.GetType()), x ),
                Tuple.Create(x.GetType().IsClass() ? x.GetType() : typeof(Nullable<>).MakeGenericType(x.GetType()), (object)null),
            })
            .Distinct()
            .Select(x => new object[] { x.Item1, x.Item2 });

        private static readonly MethodInfo MapAsValueMethod =
            typeof(When_mapping_primitive_values).GetMethod(nameof(MapAsValue), BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly MethodInfo MapAsPropertyMethod =
            typeof(When_mapping_primitive_values).GetMethod(nameof(MapAsProperty), BindingFlags.Static | BindingFlags.NonPublic);

        [Theory]
        [MemberData(nameof(TestData))]
        public void Should_map_primitive_value(Type type, object value)
        {
            var result = MapAsValueMethod.MakeGenericMethod(type).Invoke(null, new[] { value });

            if (result == null)
            {
                if (type.IsValueType())
                {
                    var message = $"value must not be null for type {type}";
                    type.IsGenericType().ShouldBeTrue(message);
                    type.GetGenericTypeDefinition().ShouldBe(typeof(Nullable<>), message);
                }
            }
            else
            {
                result.ShouldBeAssignableTo(type);
            }

            result.ShouldBe(value);
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void Should_map_primitive_value_property(Type type, object value)
        {
            var result = MapAsPropertyMethod.MakeGenericMethod(type).Invoke(null, new[] { value });

            if (result == null)
            {
                if (type.IsValueType())
                {
                    var message = $"value must not be null for type {type}";
                    type.IsGenericType().ShouldBeTrue(message);
                    type.GetGenericTypeDefinition().ShouldBe(typeof(Nullable<>), message);
                }
            }
            else
            {
                result.ShouldBeAssignableTo(type);
            }

            result.ShouldBe(value);
        }

        private static T MapAsValue<T>(T value)
        {
            var dynamicObject = new DynamicObjectMapper().MapObject(value);
            var mappedValue = new DynamicObjectMapper().Map(dynamicObject);
            return (T)mappedValue;
        }

        private static T MapAsProperty<T>(T value)
        {
            var dynamicObject = new DynamicObjectMapper().MapObject(new A<T> { Value = value });
            var mappedValue = new DynamicObjectMapper().Map(dynamicObject);
            return ((A<T>)mappedValue).Value;
        }
    }
}
