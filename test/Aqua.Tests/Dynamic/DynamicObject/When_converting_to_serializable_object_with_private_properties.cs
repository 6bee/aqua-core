// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject;

using Aqua.Dynamic;
using Shouldly;
using System.Reflection;
using Xunit;

public class When_converting_to_serializable_object_with_private_properties
{
    [Serializable]
    private class SerializableType
    {
        public int Int32Property { get; set; }

        private double DoubleProperty { get; set; }

        private string StringProperty { get; set; }
    }

    private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;
    private const int Int32Value = 11;
    private const double DoubleValue = 12.3456789;
    private const string StringValue = "eleven";

    private readonly SerializableType obj;

    public When_converting_to_serializable_object_with_private_properties()
    {
        var dynamicObject = new DynamicObject
        {
            Properties = new PropertySet
            {
                { nameof(SerializableType.Int32Property), Int32Value },
                { "DoubleProperty", DoubleValue },
                { "StringProperty", StringValue },
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
    public void Should_have_set_the_int_property()
    {
        obj.Int32Property.ShouldBe(Int32Value);
    }

#if NET8_0_OR_GREATER
    [Fact]
    public void Should_have_not_set_the_private_double_property()
    {
        GetPropertyValue("DoubleProperty").ShouldBe(default(double));
    }

    [Fact]
    public void Should_have_not_set_the_private_string_property()
    {
        GetPropertyValue("StringProperty").ShouldBe(default(string));
    }
#else // NET8_0_OR_GREATER
    [Fact]
    public void Should_have_set_the_private_double_property()
    {
        GetPropertyValue("DoubleProperty").ShouldBe(DoubleValue);
    }

    [Fact]
    public void Should_have_set_the_private_string_property()
    {
        GetPropertyValue("StringProperty").ShouldBe(StringValue);
    }
#endif // NET8_0_OR_GREATER

    private object GetPropertyValue(string propertyName)
        => typeof(SerializableType).GetProperty(propertyName, PrivateInstance).GetValue(obj);
}