// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper;

using Aqua.Dynamic;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class When_mapping_collection_from_dictionary
{
    private readonly Dictionary<string, string> source;
    private readonly IEnumerable<DynamicObject> dynamicObjects;

    public When_mapping_collection_from_dictionary()
    {
        source = new Dictionary<string, string>
        {
            { "K1", "V1" },
            { "K2", "V2" },
            { "K3", "V3" },
        };
        dynamicObjects = new DynamicObjectMapper().MapCollection(source);
    }

    [Fact]
    public void Dynamic_objects_count_should_be_three()
    {
        dynamicObjects.Count().ShouldBe(3);
    }

    [Fact]
    public void Dynamic_objects_type_property_should_be_set_to_keyvaluepair()
    {
        foreach (var dynamicObject in dynamicObjects)
        {
            dynamicObject.Type.ToType().ShouldBe(typeof(KeyValuePair<string, string>));
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
    public void Dynamic_objects_member_names_should_be_key_and_value()
    {
        foreach (var dynamicObject in dynamicObjects)
        {
            dynamicObject.PropertyNames.ShouldContain("Key");
            dynamicObject.PropertyNames.ShouldContain("Value");
        }
    }

    [Fact]
    public void Dynamic_objects_member_values_should_be_key_and_value_of_source()
    {
        for (int i = 0; i < source.Count; i++)
        {
            var dynamicObject = dynamicObjects.ElementAt(i);

            var key = source.Keys.ElementAt(i);
            var value = source.Values.ElementAt(i);

            dynamicObject["Key"].ShouldBe(key);
            dynamicObject["Value"].ShouldBe(value);
        }
    }
}