// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject;

using Aqua.Dynamic;
using Shouldly;
using Xunit;

public class When_created_using_parameterless_constructor
{
    private readonly DynamicObject dynamicObject = new();

    [Fact]
    public void Should_be_empty()
    {
        dynamicObject.PropertyCount.ShouldBe(0);
    }

    [Fact]
    public void Type_property_should_be_null()
    {
        dynamicObject.Type.ShouldBeNull();
    }
}