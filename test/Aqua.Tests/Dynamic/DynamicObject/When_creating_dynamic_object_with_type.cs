// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject;

using Aqua.Dynamic;
using Shouldly;
using Xunit;

public class When_creating_dynamic_object_with_type
{
    private class CustomClass
    {
    }

    [Fact]
    public void Should_have_type_property_set_to_specified_type()
    {
        var dynamicObject = new DynamicObject(typeof(CustomClass));

        dynamicObject.Type.ToType().ShouldBe(typeof(CustomClass));
    }
}