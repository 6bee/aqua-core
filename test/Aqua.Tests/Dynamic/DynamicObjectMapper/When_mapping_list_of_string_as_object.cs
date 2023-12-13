// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper;

using Aqua.Dynamic;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class When_mapping_list_of_string_as_object
{
    private readonly DynamicObject dynamicObject;

    public When_mapping_list_of_string_as_object()
    {
        dynamicObject = new DynamicObjectMapper().MapObject(new List<string> { "One", null });
    }

    [Fact]
    public void Dynamic_object_should_have_one_property_with_empty_name()
    {
        dynamicObject.Properties.Single().Name.ShouldBe(string.Empty);
    }

    [Fact]
    public void Dynamic_object_type_should_be_array_of_string()
    {
        dynamicObject.Type.ToType().ShouldBe(typeof(List<string>));
    }

    [Fact]
    public void Dynamic_object_should_have_items_property_with_object_array()
    {
        var items = dynamicObject.Values.Single().ShouldBeOfType<object[]>();
        items[0].ShouldBe("One");
        items[1].ShouldBeNull();
    }

    [Fact]
    public void Dynamic_object_should_result_in_list_of_string_when_mapped_back()
    {
        var list = dynamicObject.CreateObject().ShouldBeOfType<List<string>>();
        list[0].ShouldBe("One");
        list[1].ShouldBeNull();
    }
}