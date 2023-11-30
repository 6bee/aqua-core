﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper;

using Aqua.Dynamic;
using Shouldly;
using Xunit;

public class When_mapping_object_from_value_type_without_constructor
{
    private struct CustomValueType
    {
        public long Int64Property { get; set; }
    }

    private readonly DynamicObject dynamicObject;

    public When_mapping_object_from_value_type_without_constructor()
    {
        var source = new CustomValueType { Int64Property = 42L };
        dynamicObject = new DynamicObjectMapper().MapObject(source);
    }

    [Fact]
    public void Dynamic_object_type_should_be_custom_value_type()
    {
        dynamicObject.Type.ToType().ShouldBe(typeof(CustomValueType));
    }

    [Fact]
    public void Dynamic_object_property_should_hold_long_value()
    {
        dynamicObject[nameof(CustomValueType.Int64Property)].ShouldBe(42L);
    }

    [Fact]
    public void Object_property_should_hold_long_value()
    {
        var obj = new DynamicObjectMapper().Map<CustomValueType>(dynamicObject);
        obj.Int64Property.ShouldBe(42L);
    }
}
