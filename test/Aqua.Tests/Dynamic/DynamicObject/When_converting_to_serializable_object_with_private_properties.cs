﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using Shouldly;
    using System;
    using System.Reflection;
    using Xunit;

    public class When_converting_to_serializable_object_with_private_properties
    {
        [Serializable]
        private class SerializableType
        {
            public int Int32Value { get; set; }

            private double DoubleValue { get; set; }

            private string StringValue { get; set; }
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
                    { "Int32Value", Int32Value },
                    { "DoubleValue", DoubleValue },
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
        public void Should_have_the_private_double_property_set()
        {
            GetPropertyValue("DoubleValue").ShouldBe(DoubleValue);
        }

        [Fact]
        public void Should_have_the_private_string_property_set()
        {
            GetPropertyValue("StringValue").ShouldBe(StringValue);
        }

        private object GetPropertyValue(string propertyName)
            => typeof(SerializableType).GetProperty(propertyName, PrivateInstance).GetValue(obj);
    }
}
