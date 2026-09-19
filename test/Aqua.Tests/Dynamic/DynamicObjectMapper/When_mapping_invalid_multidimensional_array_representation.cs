// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper;

using Aqua.Dynamic;

public class When_mapping_invalid_multidimensional_array_representation
{
    [Fact]
    public void Missing_dimensions_should_throw()
        => Map(new Property(string.Empty, new object[] { 1 })).Message.ShouldContain("Dimensions");

    [Fact]
    public void Missing_values_should_throw()
        => Map(new Property("Dimensions", new object[] { 1, 1 })).Message.ShouldContain("empty-name value property");

    [Fact]
    public void Duplicate_dimensions_should_throw()
        => Map(
            new Property(string.Empty, new object[] { 1 }),
            new Property("Dimensions", new object[] { 1, 1 }),
            new Property("Dimensions", new object[] { 1, 1 }))
        .Message.ShouldContain("exactly one 'Dimensions'");

    [Fact]
    public void Non_collection_dimensions_should_throw()
        => Map(
            new Property(string.Empty, new object[] { 1 }),
            new Property("Dimensions", 1))
        .Message.ShouldContain("collection of array lengths");

    [Fact]
    public void Non_integral_dimension_should_throw()
        => Map(
            new Property(string.Empty, new object[] { 1 }),
            new Property("Dimensions", new object[] { 1, 1.5 }))
        .Message.ShouldContain("integral value");

    [Fact]
    public void Negative_dimension_should_throw()
        => Map(
            new Property(string.Empty, Array.Empty<object>()),
            new Property("Dimensions", new object[] { 1, -1 }))
        .Message.ShouldContain("must not be negative");

    [Fact]
    public void Wrong_rank_should_throw()
        => Map(
            new Property(string.Empty, new object[] { 1 }),
            new Property("Dimensions", new object[] { 1 }))
        .Message.ShouldContain("exactly 2 array lengths");

    [Fact]
    public void Dimensions_length_overflow_should_throw()
        => Map(
            new Property(string.Empty, Array.Empty<object>()),
            new Property("Dimensions", new object[] { int.MaxValue, 2 }))
        .Message.ShouldContain("exceeds the supported size");

    [Fact]
    public void Non_collection_values_should_throw()
        => Map(
            new Property(string.Empty, 1),
            new Property("Dimensions", new object[] { 1, 1 }))
        .Message.ShouldContain("value property must contain a collection");

    [Fact]
    public void Mismatched_value_count_should_throw()
        => Map(
            new Property(string.Empty, new object[] { 1 }),
            new Property("Dimensions", new object[] { 1, 2 }))
        .Message.ShouldContain("does not match");

    private static DynamicObjectMapperException Map(params Property[] properties)
    {
        var dynamicObject = new DynamicObject(typeof(int[,]), new PropertySet(properties));
        return Should.Throw<DynamicObjectMapperException>(() => dynamicObject.CreateObject());
    }
}
