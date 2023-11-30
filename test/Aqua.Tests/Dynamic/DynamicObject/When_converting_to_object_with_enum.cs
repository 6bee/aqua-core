// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject;

using Aqua.Dynamic;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class When_converting_to_object_with_enum
{
    private enum Custom
    {
        Value0 = 0,
        Value1 = 1,
        Value2 = 2,
        Value3 = 3,
    }

    private class ClassWithEnum
    {
        public Custom? EnumProperty { get; set; }
    }

    private readonly DynamicObject[] dynamicObjects;
    private readonly IEnumerable<ClassWithEnum> objects;

    public When_converting_to_object_with_enum()
    {
        dynamicObjects =
        [
            new DynamicObject(typeof(ClassWithEnum))
            {
                Properties = new PropertySet
                {
                    { "EnumProperty", Custom.Value1 },
                },
            },
            new DynamicObject(typeof(ClassWithEnum))
            {
                Properties = new PropertySet
                {
                    { "EnumProperty", Custom.Value2.ToString().ToUpperInvariant() },
                },
            },
            new DynamicObject(typeof(ClassWithEnum))
            {
                Properties = new PropertySet
                {
                    { "EnumProperty", (int)Custom.Value3 },
                },
            },
            new DynamicObject(typeof(ClassWithEnum))
            {
                Properties = new PropertySet
                {
                    { "EnumProperty", null },
                },
            },
        ];

        var mapper = new DynamicObjectMapper();
        objects = dynamicObjects.Select(mapper.Map<ClassWithEnum>);
    }

    [Fact]
    public void Number_of_obects_should_be_number_of_dynamic_objects()
    {
        objects.Count().ShouldBe(dynamicObjects.Length);
    }

    [Fact]
    public void Enum_property_should_be_set_according_enum_value()
    {
        objects.ElementAt(0).EnumProperty.ShouldBe(Custom.Value1);
    }

    [Fact]
    public void Enum_property_should_be_set_according_string_value()
    {
        objects.ElementAt(1).EnumProperty.ShouldBe(Custom.Value2);
    }

    [Fact]
    public void Enum_property_should_be_set_according_int_value()
    {
        objects.ElementAt(2).EnumProperty.ShouldBe(Custom.Value3);
    }

    [Fact]
    public void Enum_property_should_be_set_to_null()
    {
        objects.ElementAt(3).EnumProperty.ShouldBeNull();
    }
}
