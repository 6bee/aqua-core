// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject;

using Aqua.Dynamic;
using Shouldly;
using System;
using Xunit;

public class When_converting_to_anonymous_type
{
    private const int Int32Value = 11;
    private const double DoubleValue = 1.234567891;
    private const string StringValue = "eleven";

    private readonly DynamicObject dynamicObject;

    public When_converting_to_anonymous_type()
    {
        dynamicObject = new DynamicObject
        {
            Properties = new PropertySet
            {
                { "Int32Value", Int32Value },
                { "DoubleValue", DoubleValue },
                { "StringValue", StringValue },
            },
        };
    }

    [Fact]
    public void Should_be_convertible_to_anonymous_type_with_same_property_order()
    {
        var objType = new { Int32Value, DoubleValue, StringValue };

        var obj = dynamicObject.CreateObject(objType.GetType());

        obj.ShouldNotBeNull();
    }

    [Fact]
    public void Should_be_convertible_to_anonymous_type_with_less_properties()
    {
        var objType = new { Int32Value, StringValue };

        var obj = dynamicObject.CreateObject(objType.GetType());

        obj.ShouldNotBeNull();
    }

    [Fact]
    public void Should_not_be_convertible_to_anonymous_type_with_additional_properties()
    {
        var objType = new { Int32Value, DoubleValue, StringValue, DateTimeValue = DateTime.Now };

        var ex = Assert.Throws<DynamicObjectMapperException>(() => dynamicObject.CreateObject(objType.GetType()));

        ex.Message.ShouldStartWith("Failed to pick matching constructor for type");
    }

    [Fact]
    public void Should_be_convertible_to_anonymous_type_with_different_property_order()
    {
        var objType = new { StringValue, Int32Value, DoubleValue };

        var obj = dynamicObject.CreateObject(objType.GetType());

        obj.ShouldNotBeNull();
    }
}