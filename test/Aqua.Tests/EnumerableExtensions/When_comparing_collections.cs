// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.EnumerableExtensions;

using Aqua.EnumerableExtensions;

public class When_comparing_the_same_collection
{
    [Fact]
    public void Should_be_equal()
    {
        var collection = new int?[] { 1, 2, 3 };

        collection.CollectionEquals(collection).ShouldBeTrue();
    }
}

public class When_comparing_collections_with_equal_items_in_a_different_order
{
    [Fact]
    public void Should_be_equal()
    {
        new int?[] { 1, null, 3, 3 }.CollectionEquals([3, 1, 3, null]).ShouldBeTrue();
    }
}

public class When_comparing_collections_with_different_item_counts
{
    [Fact]
    public void Should_not_be_equal()
    {
        new int?[] { 1, null, 3 }.CollectionEquals([1, null, 3, 3]).ShouldBeFalse();
    }
}

public class When_comparing_collections_with_different_items
{
    [Fact]
    public void Should_not_be_equal()
    {
        new[] { 1, 2 }.CollectionEquals([1, 3]).ShouldBeFalse();
    }
}

public class When_comparing_null_and_empty_collections
{
    [Fact]
    public void Should_be_equal()
    {
        IEnumerable<int> collection = null;

        collection.CollectionEquals(Array.Empty<int>()).ShouldBeTrue();
    }
}

public class When_comparing_null_and_nonempty_collections
{
    [Fact]
    public void Should_not_be_equal()
    {
        IEnumerable<int> collection = null;

        collection.CollectionEquals([1]).ShouldBeFalse();
    }
}

public class When_comparing_null_collections
{
    [Fact]
    public void Should_be_equal()
    {
        IEnumerable<object> first = null;
        IEnumerable<object> second = null;

        first.CollectionEquals(second).ShouldBeTrue();
    }
}

public class When_comparing_collections_with_a_custom_comparer
{
    [Fact]
    public void Should_use_the_comparer()
    {
        new[] { "One", "Two" }.CollectionEquals(["one", "two"], StringComparer.OrdinalIgnoreCase).ShouldBeTrue();
    }
}

public class When_hashing_equal_collections_in_a_different_order
{
    [Fact]
    public void Should_return_the_same_hash_code()
    {
        new int?[] { 1, null, 3, 3 }.GetCollectionHashCode().ShouldBe(new int?[] { 3, 1, 3, null }.GetCollectionHashCode());
    }
}

public class When_hashing_collections_with_different_item_counts
{
    [Fact]
    public void Should_return_different_hash_codes()
    {
        new int?[] { 1, null, 3 }.GetCollectionHashCode().ShouldNotBe(new int?[] { 1, null, 3, 3 }.GetCollectionHashCode());
    }
}

public class When_hashing_a_null_collection
{
    [Fact]
    public void Should_match_an_empty_collection()
    {
        IEnumerable<int> collection = null;

        collection.GetCollectionHashCode().ShouldBe(Array.Empty<int>().GetCollectionHashCode());
    }
}

public class When_hashing_a_nonempty_collection_and_null
{
    [Fact]
    public void Should_return_different_hash_codes()
    {
        IEnumerable<int> collection = null;

        new[] { 1 }.GetCollectionHashCode().ShouldNotBe(collection.GetCollectionHashCode());
    }
}

public class When_hashing_collections_with_a_custom_comparer
{
    [Fact]
    public void Should_use_the_comparer()
    {
        new[] { "One" }.GetCollectionHashCode(StringComparer.OrdinalIgnoreCase).ShouldBe(new[] { "one" }.GetCollectionHashCode(StringComparer.OrdinalIgnoreCase));
    }
}
