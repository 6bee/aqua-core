// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using Shouldly;
    using System.Reflection;
    using Xunit;

    public class When_converting_to_object_with_private_properties
    {
        private class CustomType
        {
            public int Int32Property { get; set; }

            private double DoubleProperty { get; set; }

            private string StringProperty { get; set; }
        }

        private const int Int32Value = 11;
        private const double DoubleValue = 12.3456789;
        private const string StringValue = "eleven";

        private readonly CustomType obj;

        public When_converting_to_object_with_private_properties()
        {
            var dynamicObject = new DynamicObject
            {
                Properties = new PropertySet
                {
                    { nameof(CustomType.Int32Property), Int32Value },
                    { "DoubleProperty", DoubleValue },
                    { "StringProperty", StringValue },
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
            obj.Int32Property.ShouldBe(Int32Value);
        }

        [Fact]
        public void Should_have_the_double_property_not_set()
        {
            GetPropertyValue("DoubleProperty").ShouldBe(default(double));
        }

        [Fact]
        public void Should_have_the_string_property_not_set()
        {
            GetPropertyValue("StringProperty").ShouldBeNull();
        }

        private object GetPropertyValue(string propertyName)
        {
            return typeof(CustomType).GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(obj);
        }
    }
}