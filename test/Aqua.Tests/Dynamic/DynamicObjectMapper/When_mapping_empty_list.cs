// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper;

using Aqua.Dynamic;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class When_mapping_empty_list
{
    private readonly DynamicObject dynamicObject;

    public When_mapping_empty_list()
    {
        dynamicObject = DynamicObject.Create(new List<string>());
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
    public void Dynamic_object_should_have_items_property_with_empty_object_array()
    {
        dynamicObject.GetValues().Single()
            .ShouldBeOfType<object[]>()
            .Length.ShouldBe(0);
    }

    [Fact]
    public void Dynamic_object_should_result_in_empty_list_when_mapped_back()
    {
        dynamicObject.CreateObject()
            .ShouldBeOfType<List<string>>()
            .ShouldBeEmpty();
    }
}