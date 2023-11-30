﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject;

using Aqua.Dynamic;
using Shouldly;
using System;
using Xunit;

public class When_converting_to_serializable_object_with_additional_properies
{
    [Serializable]
    private class SerializableType
    {
        public int Int32Property { get; set; }

        public double DoubleProperty { get; set; }

        public DateTime? NullableDateTimeProperty { get; set; }

        public string StringValueProperty { get; set; }
    }

    private const int Int32Value = 11;
    private const string StringValue = "eleven";

    private readonly SerializableType obj;

    public When_converting_to_serializable_object_with_additional_properies()
    {
        var dynamicObject = new DynamicObject
        {
            Properties = new PropertySet
            {
                { nameof(SerializableType.Int32Property), Int32Value },
                { nameof(SerializableType.StringValueProperty), StringValue },
            },
        };

        obj = dynamicObject.CreateObject<SerializableType>();
    }

    [Fact]
    public void Should_create_an_instance()
    {
        obj.ShouldNotBeNull();
    }

    [Fact]
    public void Should_have_the_int_property_set()
    {
        obj.Int32Property.ShouldBe(Int32Value);
    }

    [Fact]
    public void Should_have_the_double_property_not_set()
    {
        obj.DoubleProperty.ShouldBe(default(double));
    }

    [Fact]
    public void Should_have_the_date_property_not_set()
    {
        obj.NullableDateTimeProperty.ShouldBeNull();
    }

    [Fact]
    public void Should_have_the_string_property_set()
    {
        obj.StringValueProperty.ShouldBe(StringValue);
    }
}
