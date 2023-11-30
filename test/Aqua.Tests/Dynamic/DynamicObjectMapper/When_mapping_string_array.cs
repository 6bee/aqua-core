// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper;

using Aqua.Dynamic;
using Shouldly;
using System.Linq;
using Xunit;

public class When_mapping_string_array
{
    private readonly DynamicObject dynamicObject;

    public When_mapping_string_array()
    {
        dynamicObject = DynamicObject.Create(new[] { "One", "Two" });
    }

    [Fact]
    public void Dynamic_object_should_have_one_property_with_empty_name()
    {
        dynamicObject.Properties.Single().Name.ShouldBe(string.Empty);
    }

    [Fact]
    public void Dynamic_object_type_should_be_array_of_string()
    {
        dynamicObject.Type.ToType().ShouldBe(typeof(string[]));
    }

    [Fact]
    public void Dynamic_object_should_have_one_property_with_object_array_value()
    {
        var array = dynamicObject.Values.Single().ShouldBeOfType<object[]>();
        array[0].ShouldBe("One");
        array[1].ShouldBe("Two");
    }

    [Fact]
    public void Dynamic_object_should_result_in_string_array_when_mapped_back()
    {
        var array = dynamicObject.CreateObject().ShouldBeOfType<string[]>();
        array[0].ShouldBe("One");
        array[1].ShouldBe("Two");
    }
}
