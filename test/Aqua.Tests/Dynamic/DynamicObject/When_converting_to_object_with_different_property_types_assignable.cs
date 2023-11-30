// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject;

using Aqua.Dynamic;
using Shouldly;
using System;
using Xunit;

/// <summary>
/// Covers mapping type missmatches for assignable types, i.e. no casting required.
/// </summary>
public class When_converting_to_object_with_different_property_types_assignable
{
    private class CustomType
    {
        public double DoubleProperty { get; set; }

        public int? NullableInt32Property { get; set; }

        public object ObjectProperty { get; set; }

        public string StringProperty { get; set; }
    }

    private const int Int32Value = 11;
    private const string StringValue = "eleven";

    private readonly DateTime DateTimeValue = DateTime.Now;
    private readonly CustomType obj;

    public When_converting_to_object_with_different_property_types_assignable()
    {
        var dynamicObject = new DynamicObject
        {
            Properties = new PropertySet
            {
                { nameof(CustomType.DoubleProperty), Int32Value },
                { nameof(CustomType.NullableInt32Property), Int32Value },
                { nameof(CustomType.ObjectProperty), DateTimeValue },
                { nameof(CustomType.StringProperty), StringValue },
            },
        };

        obj = dynamicObject.CreateObject<CustomType>();
    }

    [Fact]
    public void Should_create_an_instance()
    {
        obj.ShouldNotBeNull();
    }

    [Fact]
    public void Should_have_the_double_property_set()
    {
        obj.DoubleProperty.ShouldBe(Int32Value);
    }

    [Fact]
    public void Should_have_the_nullableint_property_set()
    {
        obj.NullableInt32Property.ShouldBe(Int32Value);
    }

    [Fact]
    public void Should_have_the_object_property_set()
    {
        obj.ObjectProperty.ShouldBe(DateTimeValue);
    }

    [Fact]
    public void Should_have_the_string_property_set()
    {
        obj.StringProperty.ShouldBe(StringValue);
    }
}