// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject;

using Aqua.Dynamic;
using Aqua.EnumerableExtensions;

public class When_checking_if_dynamic_object_is_single_value_wrapper
{
    [Fact]
    public void Empty_name_property_should_be_single_value_wrapper()
        => new Aqua.Dynamic.DynamicObject([new Property(string.Empty, 1)]).IsSingleValueWrapper().ShouldBeTrue();

    [Fact]
    public void Empty_name_property_with_metadata_should_be_single_value_wrapper()
        => new Aqua.Dynamic.DynamicObject(
        [
            new Property("Dimensions", new object[] { 1, 1 }),
            new Property(string.Empty, new object[] { 1 }),
            new Property("OtherMetadata", "value"),
        ]).IsSingleValueWrapper().ShouldBeTrue();

    [Fact]
    public void Named_properties_without_empty_name_should_not_be_single_value_wrapper()
        => new Aqua.Dynamic.DynamicObject([new Property("Value", 1)]).IsSingleValueWrapper().ShouldBeFalse();

    [Fact]
    public void Empty_properties_should_not_be_single_value_wrapper()
        => new Aqua.Dynamic.DynamicObject(new PropertySet()).IsSingleValueWrapper().ShouldBeFalse();

    [Fact]
    public void Null_properties_should_not_be_single_value_wrapper()
        => new Aqua.Dynamic.DynamicObject((PropertySet)null).IsSingleValueWrapper().ShouldBeFalse();

    [Fact]
    public void Duplicate_empty_name_properties_should_not_be_single_value_wrapper()
    {
        var dynamicObject = new Aqua.Dynamic.DynamicObject(
        [
            new Property("1", 1),
            new Property("2", 2),
        ]);
        dynamicObject.Properties.ForEach(x => x.Name = string.Empty);
        dynamicObject.IsSingleValueWrapper().ShouldBeFalse();
    }

    [Fact]
    public void Metadata_order_should_not_affect_wrapped_value_mapping()
    {
        var dynamicObject = new Aqua.Dynamic.DynamicObject(typeof(int),
        [
            new Property("Metadata", "value"),
            new Property(string.Empty, 42),
        ]);

        dynamicObject.CreateObject().ShouldBe(42);
    }
}
