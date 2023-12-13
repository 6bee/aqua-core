// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper;

using Aqua.Dynamic;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class When_mapping_object_from_object_with_collection_member
{
    private class CustomClass
    {
        public IEnumerable<object> Items { get; set; }
    }

    private class CustomReferenceType
    {
        public string StringProperty { get; set; }
    }

    private struct CustomValueType
    {
        public long Int64Property { get; set; }
    }

    private readonly DynamicObject dynamicObject;

    public When_mapping_object_from_object_with_collection_member()
    {
        var source = new CustomClass { Items = new object[] { 1, null, "test", new CustomValueType { Int64Property = 42L }, new CustomReferenceType { StringProperty = "S 2" } } };
        dynamicObject = new DynamicObjectMapper().MapObject(source);
    }

    [Fact]
    public void Dynamic_object_items_count_should_be_five()
    {
        ((IEnumerable<object>)dynamicObject["Items"]).Count().ShouldBe(5);
    }

    [Fact]
    public void Object_items_count_should_be_five()
    {
        var obj = new DynamicObjectMapper().Map<CustomClass>(dynamicObject);
        obj.Items.Count().ShouldBe(5);
    }

    [Fact]
    public void Dynamic_object_item_1_should_be_int32_value()
    {
        ((IEnumerable<object>)dynamicObject["Items"]).ElementAt(0).ShouldBe(1);
    }

    [Fact]
    public void Object_item_1_should_be_int32_value()
    {
        var obj = new DynamicObjectMapper().Map<CustomClass>(dynamicObject);
        obj.Items.ElementAt(0).ShouldBe(1);
    }

    [Fact]
    public void Dynamic_object_item_2_should_be_null()
    {
        ((IEnumerable<object>)dynamicObject["Items"]).ElementAt(1).ShouldBeNull();
    }

    [Fact]
    public void Object_item_2_should_be_null()
    {
        var obj = new DynamicObjectMapper().Map<CustomClass>(dynamicObject);
        obj.Items.ElementAt(1).ShouldBeNull();
    }

    [Fact]
    public void Dynamic_object_item_3_should_be_string_value()
    {
        ((IEnumerable<object>)dynamicObject["Items"]).ElementAt(2).ShouldBe("test");
    }

    [Fact]
    public void Object_item_3_should_be_string_value()
    {
        var obj = new DynamicObjectMapper().Map<CustomClass>(dynamicObject);
        obj.Items.ElementAt(2).ShouldBe("test");
    }

    [Fact]
    public void Dynamic_object_item_4_should_be_dynamic_object_for_value_type()
    {
        var value = ((IEnumerable<object>)dynamicObject["Items"]).ElementAt(3);
        value.ShouldBeOfType<DynamicObject>();
        ((DynamicObject)value).Type.ToType().ShouldBe(typeof(CustomValueType));
        ((DynamicObject)value)["Int64Property"].ShouldBe(42L);
    }

    [Fact]
    public void Object_item_3_should_be_value_type()
    {
        var item = new DynamicObjectMapper().Map<CustomClass>(dynamicObject).Items.ElementAt(3);
        item.ShouldBeOfType<CustomValueType>()
            .Int64Property.ShouldBe(42L);
    }

    [Fact]
    public void Dynamic_object_item_5_should_be_dynamic_object_for_reference_type()
    {
        var value = ((IEnumerable<object>)dynamicObject["Items"]).ElementAt(4);
        value.ShouldBeOfType<DynamicObject>();
        ((DynamicObject)value).Type.ToType().ShouldBe(typeof(CustomReferenceType));
        ((DynamicObject)value)["StringProperty"].ShouldBe("S 2");
    }

    [Fact]
    public void Object_item_4_should_be_reference_type()
    {
        var item = new DynamicObjectMapper().Map<CustomClass>(dynamicObject).Items.ElementAt(4);
        item.ShouldBeOfType<CustomReferenceType>()
            .StringProperty.ShouldBe("S 2");
    }
}