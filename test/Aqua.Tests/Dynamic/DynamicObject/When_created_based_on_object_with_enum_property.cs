﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject;

using Aqua.Dynamic;
using Shouldly;
using Xunit;

public class When_created_based_on_object_with_enum_property
{
    private enum Custom
    {
        Value1,
        Value2,
        Value3,
    }

    private class ClassWithEnum
    {
        public Custom EnumProperty { get; set; }
    }

    private readonly ClassWithEnum source;
    private readonly DynamicObject dynamicObject;

    public When_created_based_on_object_with_enum_property()
    {
        source = new ClassWithEnum
        {
            EnumProperty = Custom.Value2,
        };

        dynamicObject = new DynamicObject(source);
    }

    [Fact]
    public void Member_count_should_be_one()
    {
        dynamicObject.PropertyCount.ShouldBe(1);
    }

    [Fact]
    public void Member_name_should_be_name_of_property()
    {
        dynamicObject.PropertyNames.ShouldContain("EnumProperty");
    }

    [Fact]
    public void Member_value_should_be_stringvalue_of_enum_property()
    {
        var enumValueString = source.EnumProperty.ToString();
        dynamicObject["EnumProperty"].ShouldBe(enumValueString);
    }

    [Fact]
    public void Type_property_should_be_set_to_type_of_source_object()
    {
        dynamicObject.Type.ShouldNotBeNull();
        dynamicObject.Type.ToType().ShouldBe(typeof(ClassWithEnum));
    }
}
