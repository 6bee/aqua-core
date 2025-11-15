// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper;

using Aqua.Dynamic;
using Shouldly;
using Xunit;

public class When_mapping_null
{
    private class CustomClass
    {
    }

    [Fact]
    public void Map_should_be_null_for_null_dynamic_object()
    {
        var result = new DynamicObjectMapper().Map(null);
        result.ShouldBeNull();
    }

    [Fact]
    public void Map_type_should_be_null_for_null_dynamic_object()
    {
        var result = new DynamicObjectMapper().Map((DynamicObject)null, typeof(CustomClass));
        result.ShouldBeNull();
    }

    [Fact]
    public void Map_generic_type_should_be_null_for_null_dynamic_object()
    {
        var result = new DynamicObjectMapper().Map<CustomClass>((DynamicObject)null);
        result.ShouldBeNull();
    }

    [Fact]
    public void Map_generic_type_should_return_a_null_element_for_null_dynamic_object_enumerable_element()
    {
        var dynamicObjects = new DynamicObject[]
        {
            new DynamicObject(new CustomClass()),
            null,
            new DynamicObject(new CustomClass()),
        };

        var mapper = new DynamicObjectMapper();
        var result = dynamicObjects.Select(mapper.Map<CustomClass>);

        result.ShouldNotBeNull();
        result.Count().ShouldBe(3);
        result.ElementAt(0).ShouldNotBeNull();
        result.ElementAt(1).ShouldBeNull();
        result.ElementAt(2).ShouldNotBeNull();
    }

    [Fact]
    public void Map_type_should_return_a_null_element_for_null_dynamic_object_enumerable_element()
    {
        var dynamicObjects = new DynamicObject[]
        {
            new DynamicObject(new CustomClass()),
            null,
            new DynamicObject(new CustomClass()),
        };

        var mapper = new DynamicObjectMapper();
        var result = dynamicObjects.Select(x => mapper.Map(x, typeof(CustomClass)));

        result.ShouldNotBeNull();
        result.Count().ShouldBe(3);
        result.ElementAt(0).ShouldBeOfType<CustomClass>();
        result.ElementAt(1).ShouldBeNull();
        result.ElementAt(2).ShouldBeOfType<CustomClass>();
    }

    [Fact]
    public void Map_should_be_null_for_null_object()
    {
        var result = new DynamicObjectMapper().MapObject(null);
        result.ShouldBeNull();
    }

    [Fact]
    public void Map_should_be_null_for_null_object_enumerable()
    {
        var objects = new object[]
        {
            new CustomClass(),
            null,
            new CustomClass(),
        };

        var result = new DynamicObjectMapper().MapCollection(objects);

        result.ShouldNotBeNull();
        result.Count().ShouldBe(3);
        result[0].ShouldBeOfType<DynamicObject>();
        result[1].ShouldBeNull();
        result[2].ShouldBeOfType<DynamicObject>();
    }

    [Fact]
    public void Map_array_as_object_should_set_item_null_for_null_reference()
    {
        var objects = new object[]
        {
            new CustomClass(),
            null,
            new CustomClass(),
        };

        var result = new DynamicObjectMapper().MapObject(objects);

        var array = result.Get<object[]>();
        array.Length.ShouldBe(3);
        array[0].ShouldBeOfType<DynamicObject>();
        array[1].ShouldBeNull();
        array[2].ShouldBeOfType<DynamicObject>();
    }

    [Fact]
    public void Map_null_to_nullable_valuetype_should_return_null()
    {
        int? result = new DynamicObjectMapper().Map<int?>((DynamicObject)null);

        result.ShouldBeNull();
        result.HasValue.ShouldBeFalse();
    }

    [Fact]
    public void Map_null_to_valuetype_should_throw()
    {
        Should.Throw<NullReferenceException>(() => _ = new DynamicObjectMapper().Map<int>((DynamicObject)null));
    }
}