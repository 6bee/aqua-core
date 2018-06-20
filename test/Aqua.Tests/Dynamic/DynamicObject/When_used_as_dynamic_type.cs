// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using Xunit;

    public class When_used_as_dynamic_type
    {
        private class A
        {
            public B B { get; set; }
        }

        private class B
        {
            public double DoubleValue { get; set; }
        }

        private const int Int32Value = 11;
        private const double DoubleValue = 1.234567891;
        private const string StringValue = "eleven";

        private readonly dynamic dynamicObject;

        public When_used_as_dynamic_type()
        {
            dynamicObject = new DynamicObject();

            dynamicObject.Int32Value = Int32Value;

            dynamicObject.DoubleValue = DoubleValue;

            dynamicObject.StringValue = StringValue;

            dynamicObject.A = new A();

            dynamicObject.A.B = new B();

            dynamicObject.A.B.DoubleValue = DoubleValue;
        }

        [Fact]
        public void Int_property_should_have_been_set()
        {
            Assert.IsType<int>(dynamicObject.Int32Value);
            Assert.Equal(Int32Value, (int)dynamicObject.Int32Value);
        }

        [Fact]
        public void Double_property_should_have_been_set()
        {
            Assert.IsType<double>(dynamicObject.DoubleValue);
            Assert.Equal(DoubleValue, (double)dynamicObject.DoubleValue);
        }

        [Fact]
        public void String_property_should_have_been_set()
        {
            Assert.IsType<string>(dynamicObject.StringValue);
            Assert.Equal(StringValue, (string)dynamicObject.StringValue);
        }

        [Fact]
        public void Reference_property_should_have_been_set()
        {
            Assert.IsType<A>(dynamicObject.A);
            Assert.IsType<B>(dynamicObject.A.B);
            Assert.IsType<double>(dynamicObject.A.B.DoubleValue);
            Assert.Equal(DoubleValue, (double)dynamicObject.A.B.DoubleValue);
        }
    }
}
