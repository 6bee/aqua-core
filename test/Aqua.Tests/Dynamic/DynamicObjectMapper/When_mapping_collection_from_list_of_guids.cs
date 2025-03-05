// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper;

using Aqua.Dynamic;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class When_mapping_collection_from_list_of_guids
{
    private readonly List<Guid> source;
    private readonly IEnumerable<DynamicObject> dynamicObjects;

    public When_mapping_collection_from_list_of_guids()
    {
        source = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        dynamicObjects = new DynamicObjectMapper().MapCollection(source);
    }

    [Fact]
    public void Dynamic_objects_count_should_be_three()
    {
        dynamicObjects.Count().ShouldBe(3);
    }

    [Fact]
    public void Dynamic_objects_type_property_should_be_set_to_guid()
    {
        foreach (var dynamicObject in dynamicObjects)
        {
            dynamicObject.Type.ToType().ShouldBe(typeof(Guid));
        }
    }

    [Fact]
    public void Dynamic_objects_should_have_one_member()
    {
        foreach (var dynamicObject in dynamicObjects)
        {
            dynamicObject.PropertyCount.ShouldBe(1);
        }
    }

    [Fact]
    public void Dynamic_objects_member_name_should_be_empty_string()
    {
        foreach (var dynamicObject in dynamicObjects)
        {
            dynamicObject.GetPropertyNames().Single().ShouldBe(string.Empty);
        }
    }

    [Fact]
    public void Dynamic_objects_member_values_should_be_value_of_source()
    {
        for (int i = 0; i < source.Count; i++)
        {
            var dynamicObject = dynamicObjects.ElementAt(i);
            var value = source[i];

            dynamicObject[string.Empty].ShouldBe(value);
        }
    }
}