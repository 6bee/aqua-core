// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject;

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
        public double DoubleProperty { get; set; }
    }

    private const int Int32Value = 11;
    private const double DoubleValue = 1.234567891;
    private const string StringValue = "eleven";

    private readonly dynamic dynamicObject;

    public When_used_as_dynamic_type()
    {
        dynamicObject = new DynamicObject();

        dynamicObject.Int32Property = Int32Value;

        dynamicObject.DoubleProperty = DoubleValue;

        dynamicObject.StringProperty = StringValue;

        dynamicObject.A = new A();

        dynamicObject.A.B = new B();

        dynamicObject.A.B.DoubleProperty = DoubleValue;
    }

    [Fact]
    public void Int_property_should_have_been_set()
    {
        Assert.IsType<int>(dynamicObject.Int32Property);
        Assert.Equal(Int32Value, (int)dynamicObject.Int32Property);
    }

    [Fact]
    public void Double_property_should_have_been_set()
    {
        Assert.IsType<double>(dynamicObject.DoubleProperty);
        Assert.Equal(DoubleValue, (double)dynamicObject.DoubleProperty);
    }

    [Fact]
    public void String_property_should_have_been_set()
    {
        Assert.IsType<string>(dynamicObject.StringProperty);
        Assert.Equal(StringValue, (string)dynamicObject.StringProperty);
    }

    [Fact]
    public void Reference_property_should_have_been_set()
    {
        Assert.IsType<A>(dynamicObject.A);
        Assert.IsType<B>(dynamicObject.A.B);
        Assert.IsType<double>(dynamicObject.A.B.DoubleProperty);
        Assert.Equal(DoubleValue, (double)dynamicObject.A.B.DoubleProperty);
    }
}
