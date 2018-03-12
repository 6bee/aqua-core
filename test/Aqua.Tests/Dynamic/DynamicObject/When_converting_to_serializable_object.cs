// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using Shouldly;
    using System;
    using Xunit;

    public class When_converting_to_serializable_object
    {
        [Serializable]
        private class SerializableType
        {
            public int Int32Value { get; set; }

            public string StringValue { get; set; }
        }

        private const int Int32Value = 11;
        private const string StringValue = "eleven";

        private readonly SerializableType obj;

        public When_converting_to_serializable_object()
        {
            var dynamicObject = new DynamicObject()
            {
                Properties = new PropertySet
                {
                    { "Int32Value", Int32Value },
                    { "StringValue", StringValue },
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
            obj.Int32Value.ShouldBe(Int32Value);
        }

        [Fact]
        public void Should_have_the_string_property_set()
        {
            obj.StringValue.ShouldBe(StringValue);
        }
    }
}
