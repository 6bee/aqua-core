// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using Shouldly;
    using Xunit;

    public class When_converting_to_object_with_private_property_setter
    {
        private class CustomType
        {
            public int Int32Property { get; set; }

            public double DoubleProperty { get; private set; }

            public string StringProperty { get; private set; }
        }

        private const int Int32Value = 11;
        private const double DoubleValue = 12.3456789;
        private const string StringValue = "eleven";

        private readonly CustomType obj;

        public When_converting_to_object_with_private_property_setter()
        {
            var dynamicObject = new DynamicObject
            {
                Properties = new PropertySet
                {
                    { nameof(CustomType.Int32Property), Int32Value },
                    { nameof(CustomType.DoubleProperty), DoubleValue },
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
            obj.Int32Property.ShouldBe(Int32Value);
        }

        [Fact]
        public void Should_have_the_double_property_set()
        {
            obj.DoubleProperty.ShouldBe(DoubleValue);
        }

        [Fact]
        public void Should_have_the_string_property_set()
        {
            obj.StringProperty.ShouldBe(StringValue);
        }
    }
}