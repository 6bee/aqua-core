// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper
{
    using Aqua.Dynamic;
    using Aqua.TypeSystem.Extensions;
    using Shouldly;
    using System;
    using System.Numerics;
    using System.Reflection;
    using Xunit;

    public class When_mapping_native_values
    {
        private class A<T>
        {
            public T Value { get; set; }
        }

        private static readonly MethodInfo MapAsValueMethod =
            typeof(When_mapping_native_values).GetMethod(nameof(MapAsValue), BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly MethodInfo MapAsPropertyMethod =
            typeof(When_mapping_native_values).GetMethod(nameof(MapAsProperty), BindingFlags.Static | BindingFlags.NonPublic);

        [SkippableTheory]
        [MemberData(nameof(TestData.NativeValues), MemberType = typeof(TestData))]
        [MemberData(nameof(TestData.NativeValueArrays), MemberType = typeof(TestData))]
        public void Should_map_native_value(Type type, object value)
        {
            SkipOnNetCoreApp1_0.If(type.Is<Complex>(), "Complex fails on CORE CLR 1.0.1 on Ubuntu (travis-ci)");

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

        [SkippableTheory]
        [MemberData(nameof(TestData.NativeValues), MemberType = typeof(TestData))]
        [MemberData(nameof(TestData.NativeValueArrays), MemberType = typeof(TestData))]
        public void Should_map_native_value_property(Type type, object value)
        {
            SkipOnNetCoreApp1_0.If(type.Is<Complex>(), "Complex fails on CORE CLR 1.0.1 on Ubuntu (travis-ci)");

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
