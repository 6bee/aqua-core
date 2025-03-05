// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject;

using Aqua.Dynamic;
using Shouldly;
using System.Collections.Generic;
using Xunit;

public class When_created_based_on_a_keyvaluepair
{
    private readonly KeyValuePair<string, string> source;
    private readonly DynamicObject dynamicObject;

    public When_created_based_on_a_keyvaluepair()
    {
        source = new KeyValuePair<string, string>("K1", "V1");
        dynamicObject = new DynamicObject(source);
    }

    [Fact]
    public void Type_property_should_be_set_to_keyvaluepair()
    {
        dynamicObject.Type.ToType().ShouldBe(typeof(KeyValuePair<string, string>));
    }

    [Fact]
    public void Should_have_two_members()
    {
        dynamicObject.PropertyCount.ShouldBe(2);
    }

    [Fact]
    public void Member_names_should_be_key_and_value()
    {
        dynamicObject.GetPropertyNames().ShouldContain("Key");
        dynamicObject.GetPropertyNames().ShouldContain("Value");
    }

    [Fact]
    public void Member_values_should_be_key_and_value_of_source()
    {
        dynamicObject["Key"].ShouldBe(source.Key);
        dynamicObject["Value"].ShouldBe(source.Value);
    }
}