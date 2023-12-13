﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper;

using Aqua.Dynamic;
using Shouldly;
using System;
using System.Globalization;
using System.Reflection;
using Xunit;

public class When_mapping_native_values
{
    private class A<T>
    {
        public T Value { get; set; }
    }

    private const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;

    private static readonly MethodInfo MapAsValueMethod =
        typeof(When_mapping_native_values).GetMethod(nameof(MapAsValue), PrivateStatic);

    private static readonly MethodInfo MapAsPropertyMethod =
        typeof(When_mapping_native_values).GetMethod(nameof(MapAsProperty), PrivateStatic);

    [Theory]
    [MemberData(nameof(TestData.TestValues), MemberType = typeof(TestData))]
    [MemberData(nameof(TestData.TestValueArrays), MemberType = typeof(TestData))]
    [MemberData(nameof(TestData.TestValueLists), MemberType = typeof(TestData))]
    public void Should_map_native_value(Type type, object value, CultureInfo culture)
    {
        using var cultureContext = culture.CreateContext();

        var result = MapAsValueMethod.MakeGenericMethod(type).Invoke(null, [value]);

        if (result is null)
        {
            if (type.IsValueType)
            {
                var message = $"value must not be null for type {type}";
                type.IsGenericType.ShouldBeTrue(message);
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
    [MemberData(nameof(TestData.TestValues), MemberType = typeof(TestData))]
    [MemberData(nameof(TestData.TestValueArrays), MemberType = typeof(TestData))]
    [MemberData(nameof(TestData.TestValueLists), MemberType = typeof(TestData))]
    public void Should_map_native_value_property(Type type, object value, CultureInfo culture)
    {
        using var cultureContext = culture.CreateContext();

        var result = MapAsPropertyMethod.MakeGenericMethod(type).Invoke(null, [value]);

        if (result is null)
        {
            if (type.IsValueType)
            {
                var message = $"value must not be null for type {type}";
                type.IsGenericType.ShouldBeTrue(message);
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