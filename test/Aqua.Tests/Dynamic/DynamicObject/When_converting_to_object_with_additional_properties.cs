// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject;

using Aqua.Dynamic;
using Shouldly;
using Xunit;

public class When_converting_to_object_with_additional_properties
{
    private class CustomType
    {
        public int Int32Proeprty { get; set; }

        public double DoubleProperty { get; set; }

        public DateTime? NullableDateTimeProperty { get; set; }

        public string StringProperty { get; set; }
    }

    private const int Int32Value = 11;
    private const string StringValue = "eleven";

    private readonly CustomType obj;

    public When_converting_to_object_with_additional_properties()
    {
        var dynamicObject = new DynamicObject
        {
            Properties = new PropertySet
            {
                { nameof(CustomType.Int32Proeprty), Int32Value },
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
    public void Should_have_the_int_property_set()
    {
        obj.Int32Proeprty.ShouldBe(Int32Value);
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
        obj.StringProperty.ShouldBe(StringValue);
    }
}