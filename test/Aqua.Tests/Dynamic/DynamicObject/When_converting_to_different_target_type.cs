// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using Shouldly;
    using System.Diagnostics.CodeAnalysis;
    using Xunit;

    public class When_converting_to_different_target_type
    {
        [SuppressMessage("Major Code Smell", "S125:Sections of code should not be commented out", Justification = "For clarity purpose")]
        private class SourceType
        {
            // public int Int32Value { get; set; }
            // public string StringValue { get; set; }
        }

        private class TargetType
        {
            public int Int32Value { get; set; }

            public string StringValue { get; set; }
        }

        private const int Int32Value = 11;
        private const string StringValue = "eleven";

        private readonly TargetType obj;

        public When_converting_to_different_target_type()
        {
            var dynamicObject = new DynamicObject(typeof(SourceType))
            {
                Properties = new PropertySet
                {
                    { "StringValue", StringValue },
                    { "Int32Value", Int32Value },
                },
            };

            obj = dynamicObject.CreateObject(typeof(TargetType)) as TargetType;
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
        public void Should_have_the_string_property_set()
        {
            obj.StringValue.ShouldBe(StringValue);
        }
    }
}