// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper;

using Aqua.Dynamic;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class When_mapping_collection_from_list_of_custom_reference_type
{
    private class CustomReferenceType
    {
        public int Int32Property { get; set; }

        public string StringProperty { get; set; }
    }

    private readonly List<CustomReferenceType> source;
    private readonly IEnumerable<DynamicObject> dynamicObjects;

    public When_mapping_collection_from_list_of_custom_reference_type()
    {
        source =
        [
            new CustomReferenceType { Int32Property = 1, StringProperty = "One" },
            new CustomReferenceType { Int32Property = 2, StringProperty = "Two" },
        ];
        dynamicObjects = new DynamicObjectMapper().MapCollection(source);
    }

    [Fact]
    public void Dynamic_objects_count_should_be_two()
    {
        dynamicObjects.Count().ShouldBe(2);
    }

    [Fact]
    public void Dynamic_objects_type_property_should_be_set_to_custom_reference_type()
    {
        foreach (var dynamicObject in dynamicObjects)
        {
            dynamicObject.Type.ToType().ShouldBe(typeof(CustomReferenceType));
        }
    }

    [Fact]
    public void Dynamic_objects_should_have_two_members()
    {
        foreach (var dynamicObject in dynamicObjects)
        {
            dynamicObject.PropertyCount.ShouldBe(2);
        }
    }

    [Fact]
    public void Dynamic_objects_member_names_should_be_property_names()
    {
        foreach (var dynamicObject in dynamicObjects)
        {
            dynamicObject.GetPropertyNames().ElementAt(0).ShouldBe("Int32Property");
            dynamicObject.GetPropertyNames().ElementAt(1).ShouldBe("StringProperty");
        }
    }

    [Fact]
    public void Dynamic_objects_member_values_should_be_key_and_value_of_source()
    {
        for (int i = 0; i < source.Count; i++)
        {
            var dynamicObject = dynamicObjects.ElementAt(i);

            var intValue = source[i].Int32Property;
            var stringValue = source[i].StringProperty;

            dynamicObject["Int32Property"].ShouldBe(intValue);
            dynamicObject["StringProperty"].ShouldBe(stringValue);
        }
    }
}