// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

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
            public int Int32Property { get; set; }

            public double? NullableDoubleProperty { get; set; }

            public string StringProperty { get; set; }

            public B BProperty { get; set; }

            public C CProperty { get; set; }

            public D DProperty { get; set; }
        }

        private const long Longvalue = 12345L;
        private const double DoubleValue = 12.3456789;
        private const int IntValue = (int)DoubleValue;
        private const string StringValue = "eleven";

        private readonly CustomType obj;

        public When_converting_to_object_with_different_property_types_unassignable()
        {
            var dynamicObject = new DynamicObject
            {
                Properties = new PropertySet
                {
                    { nameof(CustomType.Int32Property), DoubleValue },
                    { nameof(CustomType.NullableDoubleProperty), Longvalue },
                    { nameof(CustomType.StringProperty), StringValue },
                    { nameof(CustomType.BProperty), new A() },
                    { nameof(CustomType.CProperty), new A() },
                    { nameof(CustomType.DProperty), new A() },
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
            obj.Int32Property.ShouldBe(IntValue); // double automatically converted into int
        }

        [Fact]
        public void Should_have_the_nullabledouble_property_not_set()
        {
            obj.NullableDoubleProperty.ShouldBeNull(); // long cannot be automatically converted into Nullable<double>
        }

        [Fact]
        public void Should_have_the_string_property_set()
        {
            obj.StringProperty.ShouldBe(StringValue);
        }

        [Fact]
        public void Should_have_the_property_of_type_B_not_set()
        {
            obj.BProperty.ShouldBeNull(); // cannot assign A to B
        }

        [Fact]
        public void Should_have_the_property_of_type_C()
        {
            obj.CProperty.ShouldBeOfType<C>(); // created C using explicit conversion
        }

        [Fact]
        public void Should_have_the_property_of_type_D()
        {
            obj.DProperty.ShouldBeOfType<D>(); // created D using implicit conversion
        }
    }
}