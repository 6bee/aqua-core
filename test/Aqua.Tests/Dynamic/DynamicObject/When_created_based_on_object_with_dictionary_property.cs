// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject;

using Aqua.Dynamic;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class When_created_based_on_object_with_dictionary_property
{
    private class ClassWithDictionaryProperty
    {
        public IDictionary<string, string> Dictionary { get; set; }
    }

    private readonly ClassWithDictionaryProperty source;
    private readonly DynamicObject dynamicObject;

    public When_created_based_on_object_with_dictionary_property()
    {
        source = new ClassWithDictionaryProperty
        {
            Dictionary = new Dictionary<string, string>
            {
                { "K1", "V1" },
                { "K2", "V2" },
                { "K3", "V3" },
            },
        };

        dynamicObject = new DynamicObject(source);
    }

    [Fact]
    public void Type_property_should_be_set_to_custom_class()
    {
        dynamicObject.Type.ToType().ShouldBe(typeof(ClassWithDictionaryProperty));
    }

    [Fact]
    public void Should_have_a_single_member()
    {
        dynamicObject.PropertyCount.ShouldBe(1);
    }

    [Fact]
    public void Member_name_should_be_name_of_property()
    {
        dynamicObject.GetPropertyNames().Single().ShouldBe("Dictionary");
    }

    [Fact]
    public void Dictionary_property_should_be_object_array()
    {
        dynamicObject["Dictionary"].ShouldBeOfType<DynamicObject[]>();
    }

    [Fact]
    public void Dictionary_property_should_have_three_elements()
    {
        ((object[])dynamicObject["Dictionary"]).Length.ShouldBe(3);
    }

    [Fact]
    public void All_dictionary_elements_should_be_dynamic_objects()
    {
        foreach (var element in (object[])dynamicObject["Dictionary"])
        {
            element.ShouldBeOfType<DynamicObject>();
        }
    }

    [Fact]
    public void All_dynamic_dictionary_elements_should_have_type_set_to_keyvaluepair()
    {
        foreach (DynamicObject element in (object[])dynamicObject["Dictionary"])
        {
            element.Type.ToType().ShouldBe(typeof(KeyValuePair<string, string>));
        }
    }

    [Fact]
    public void All_dynamic_key_value_pairs_objects_should_have_two_members()
    {
        foreach (DynamicObject element in (object[])dynamicObject["Dictionary"])
        {
            element.PropertyCount.ShouldBe(2);
        }
    }

    [Fact]
    public void All_dynamic_key_value_pairs_objects_should_have_key_and_value_member()
    {
        foreach (DynamicObject element in (object[])dynamicObject["Dictionary"])
        {
            element.GetPropertyNames().ShouldContain("Key");
            element.GetPropertyNames().ShouldContain("Value");
        }
    }

    [Fact]
    public void Dynamic_keys_and_values_should_match_source_values()
    {
        for (int i = 0; i < source.Dictionary.Count; i++)
        {
            DynamicObject element = (DynamicObject)((object[])dynamicObject["Dictionary"])[i];

            var key = source.Dictionary.Keys.ElementAt(i);
            var value = source.Dictionary.Values.ElementAt(i);

            element["Key"].ShouldBe(key);
            element["Value"].ShouldBe(value);
        }
    }
}