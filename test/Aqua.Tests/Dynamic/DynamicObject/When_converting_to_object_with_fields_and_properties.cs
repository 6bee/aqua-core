// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject;

using Aqua.Dynamic;
using Shouldly;
using System.Reflection;
using Xunit;

public class When_converting_to_object_with_fields_and_properties
{
    private class CustomType
    {
#pragma warning disable
        public readonly string StringValue = "Default";
        private string PrivateStringValue;
        public DateTime Date;
#pragma warning enable

        private double DoubleValue { get; set; }

        public int Int32Value { get; set; }

        public string RedundantValue => StringValue;
    }

    private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;
    private const int Int32Value = 11;
    private const double DoubleValue = 12.3456789;
    private const string StringValue1 = "Foo";
    private const string StringValue2 = "Bar";
    private static readonly DateTime Date = new DateTime(2002, 2, 13);

    private readonly CustomType obj;

    public When_converting_to_object_with_fields_and_properties()
    {
        var dynamicObject = new DynamicObject
        {
            Properties = new PropertySet
            {
                { "Int32Value", Int32Value },
                { "DoubleValue", DoubleValue },
                { "StringValue", StringValue1 },
                { "RedundantValue", StringValue2 },
                { "Date", Date },
                { "PrivateStringValue", StringValue2 },
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
        obj.Int32Value.ShouldBe(Int32Value);
    }

    [Fact]
    public void Should_have_the_double_property_not_set()
    {
        GetPropertyValue("DoubleValue").ShouldBe(default(double));
    }

    [Fact]
    public void Should_have_the_readonly_string_field_not_set()
    {
        obj.StringValue.ShouldBe("Default");
    }

    [Fact]
    public void Should_have_the_private_string_field_not_set()
    {
        GetFieldValue("PrivateStringValue").ShouldBeNull();
    }

    [Fact]
    public void Should_have_the_readonly_string_property_not_set()
    {
        obj.RedundantValue.ShouldBe("Default");
    }

    [Fact]
    public void Should_have_the_date_property_set()
    {
        obj.Date.ShouldBe(Date);
    }

    private object GetFieldValue(string propertyName)
        => typeof(CustomType).GetField(propertyName, PrivateInstance).GetValue(obj);

    private object GetPropertyValue(string propertyName)
        => typeof(CustomType).GetProperty(propertyName, PrivateInstance).GetValue(obj);
}