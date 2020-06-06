﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using Shouldly;
    using Xunit;

    /// <summary>
    /// Covers mapping type missmatches for types which allow exlicit conversion only.
    /// </summary>
    public class When_converting_to_object_with_different_property_types_unassignable
    {
        private class A
        {
        }

        private class B
        {
        }

        private class C
        {
            public static explicit operator C(A a)
            {
                return new C();
            }
        }

        private class D
        {
            public static implicit operator D(A a)
            {
                return new D();
            }
        }

        private class CustomType
        {
            public int Int32Value { get; set; }

            public double? NullableDoubleValue { get; set; }

            public string StringValue { get; set; }

            public B BProperty { get; set; }

            public C CProperty { get; set; }

            public D DProperty { get; set; }
        }

        private const long Longvalue = 12345L;
        private const double DoubleValue = 12.3456789;
        private const string StringValue = "eleven";

        private readonly CustomType obj;

        public When_converting_to_object_with_different_property_types_unassignable()
        {
            var dynamicObject = new DynamicObject
            {
                Properties = new PropertySet
                {
                    { "NumericValue", DoubleValue },
                    { "NullableDoubleValue", Longvalue },
                    { "StringValue", StringValue },
                    { "BProperty", new A() },
                    { "CProperty", new A() },
                    { "DProperty", new A() },
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
        public void Should_have_the_int_property_not_set()
        {
            obj.Int32Value.ShouldBe(default(int)); // double cannot be automatically converted into int
        }

        [Fact]
        public void Should_have_the_nullabledouble_property_not_set()
        {
            obj.NullableDoubleValue.ShouldBeNull(); // long cannot be automatically converted into Nullable<double>
        }

        [Fact]
        public void Should_have_the_string_property_set()
        {
            obj.StringValue.ShouldBe(StringValue);
        }

        [Fact]
        public void Should_have_the_property_of_type_B_not_set()
        {
            obj.BProperty.ShouldBeNull(); // cannot assign A to B
        }

        [Fact]
        public void Should_have_the_property_of_type_C()
        {
            obj.CProperty.ShouldBeOfType<C>();
        }

        [Fact]
        public void Should_have_the_property_of_type_D()
        {
            obj.DProperty.ShouldBeOfType<D>();
        }
    }
}