// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using Shouldly;
    using System;
    using Xunit;

    public class When_created_based_on_object_with_fields_and_properties
    {
        private class CustomType
        {
            public CustomType(string s1, string s2, int i, double d)
            {
                StringValue = s1;
                PrivateStringValue = s2;
                Int32Value = i;
                DoubleValue = d;
            }

            public readonly string StringValue = null;
            private string PrivateStringValue;
            public DateTime Date;

            private double DoubleValue { get; }

            public int Int32Value { get; }

            public string RedundantValue => StringValue;
        }

        private const int Int32Value = 11;
        private const double DoubleValue = 12.3456789;
        private const string StringValue1 = "Foo";
        private const string StringValue2 = "Bar";
        private static readonly DateTime Date = new DateTime(2002, 2, 13);

        private readonly DynamicObject dynamicObject;

        public When_created_based_on_object_with_fields_and_properties()
        {
            var source = new CustomType(StringValue1, StringValue2, Int32Value, DoubleValue)
            {
                Date = Date,
            };

            dynamicObject = new DynamicObject(source);
        }

        [Fact]
        public void Should_create_a_dynamic_object_instance_with_four_properties()
        {
            dynamicObject.Properties.Count.ShouldBe(4);
        }

        [Fact]
        public void Should_have_the_string_set_from_public_readonly_filed()
        {
            dynamicObject["StringValue"].ShouldBe(StringValue1);
        }

        [Fact]
        public void Should_have_the_date_value_set_from_public_field()
        {
            dynamicObject["Date"].ShouldBe(Date);
        }

        [Fact]
        public void Should_have_the_int_value_set_from_public_readonly__property()
        {
            dynamicObject["Int32Value"].ShouldBe(Int32Value);
        }

        [Fact]
        public void Should_have_the_string_value_set_from_readonly_property()
        {
            dynamicObject["RedundantValue"].ShouldBe(StringValue1);
        }
    }
}