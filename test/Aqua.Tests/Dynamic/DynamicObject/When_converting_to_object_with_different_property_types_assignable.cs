// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject
{
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
            public double DoubleValue { get; set; }

            public int? NullableIntValue { get; set; }

            public object Timestamp { get; set; }

            public string StringValue { get; set; }
        }

        private const int Int32Value = 11;
        private const string StringValue = "eleven";
        private readonly DateTime Timestamp = DateTime.Now;

        private readonly CustomType obj;

        public When_converting_to_object_with_different_property_types_assignable()
        {
            var dynamicObject = new DynamicObject
            {
                Properties = new PropertySet
                {
                    { "DoubleValue", Int32Value },
                    { "NullableIntValue", Int32Value },
                    { "Timestamp", Timestamp },
                    { "StringValue", StringValue },
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
            obj.DoubleValue.ShouldBe(Int32Value);
        }

        [Fact]
        public void Should_have_the_nullableint_property_set()
        {
            obj.NullableIntValue.ShouldBe(Int32Value);
        }

        [Fact]
        public void Should_have_the_object_property_set()
        {
            obj.Timestamp.ShouldBe(Timestamp);
        }

        [Fact]
        public void Should_have_the_string_property_set()
        {
            obj.StringValue.ShouldBe(StringValue);
        }
    }
}