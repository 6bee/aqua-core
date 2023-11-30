// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Extensions.EnumerableExtensions;

using Aqua.EnumerableExtensions;
using Shouldly;
using System.Collections.Generic;
using Xunit;

public class When_comparing_collections
{
    [Fact]
    public void Same_collection_instance_should_be_equal()
    {
        var list = new int?[] { 1, 2, 3 };

        list.CollectionEquals(list).ShouldBeTrue();
    }

    [Fact]
    public void Same_collection_instamce_should_have_same_hash_code()
    {
        var list = new int?[] { 1, 2, 3 };

        list.GetCollectionHashCode().ShouldBe(list.GetCollectionHashCode());
    }

    [Fact]
    public void Identical_collection_should_be_equal()
    {
        var list1 = new int?[] { 1, 2, 3 };
        var list2 = new int?[] { 1, 2, 3 };

        list1.CollectionEquals(list2).ShouldBeTrue();
    }

    [Fact]
    public void Identical_collection_should_have_same_hash_code()
    {
        var list1 = new int?[] { 1, 2, 3 };
        var list2 = new int?[] { 1, 2, 3 };

        list1.GetCollectionHashCode().ShouldBe(list2.GetCollectionHashCode());
    }

    [Fact]
    public void Unordered_collection_should_be_equal()
    {
        var list1 = new int?[] { 1, 2, 3 };
        var list2 = new int?[] { 1, 3, 2 };

        list1.CollectionEquals(list2).ShouldBeTrue();
    }

    [Fact]
    public void Unordered_collection_should_have_same_hash_code()
    {
        var list1 = new int?[] { 1, 2, 3, 3 };
        var list2 = new int?[] { 3, 1, 3, 2 };

        list1.GetCollectionHashCode().ShouldBe(list2.GetCollectionHashCode());
    }

    [Fact]
    public void Collection_with_null_element_should_be_equal()
    {
        var list1 = new int?[] { 1, null, 3, 3 };
        var list2 = new int?[] { 3, 1, 3, null };

        list1.CollectionEquals(list2).ShouldBeTrue();
    }

    [Fact]
    public void Collection_with_null_should_have_same_hash_code()
    {
        var list1 = new int?[] { 1, null, 3, 3 };
        var list2 = new int?[] { 3, 1, 3, null };

        list1.GetCollectionHashCode().ShouldBe(list2.GetCollectionHashCode());
    }

    [Fact]
    public void Collection_with_null_only_should_have_same_hash_code()
    {
        var list1 = new int?[] { null, null };
        var list2 = new int?[] { null, null };

        list1.GetCollectionHashCode().ShouldBe(list2.GetCollectionHashCode());
    }

    [Fact]
    public void Collection_with_different_number_of_null_only_should_have_same_hash_code()
    {
        var list1 = new int?[] { null, null };
        var list2 = new int?[] { null, null, null };

        list1.GetCollectionHashCode().ShouldNotBe(list2.GetCollectionHashCode());
    }

    [Fact]
    public void Two_null_reference_collections_should_be_equal()
    {
        var list1 = default(IEnumerable<object>);
        var list2 = default(IEnumerable<object>);

        list1.CollectionEquals(list2).ShouldBeTrue();
    }

    [Fact]
    public void Two_null_reference_collections_should_have_same_hash_code()
    {
        var list1 = default(IEnumerable<object>);
        var list2 = default(IEnumerable<object>);

        list1.GetCollectionHashCode().ShouldBe(list2.GetCollectionHashCode());
    }

    [Fact]
    public void Empty_collection_should_be_equal_to_null()
    {
        var list1 = new int[0];
        var list2 = default(IEnumerable<int>);

        list1.CollectionEquals(list2).ShouldBeTrue();
    }

    [Fact]
    public void Empty_collection_should_have_same_hash_code_compared_to_null()
    {
        var list1 = new int[0];
        var list2 = default(IEnumerable<int>);

        list1.GetCollectionHashCode().ShouldBe(list2.GetCollectionHashCode());
    }

    [Fact]
    public void Collection_should_not_be_equal_to_null()
    {
        var list1 = new[] { 1 };
        var list2 = default(IEnumerable<int>);

        list1.CollectionEquals(list2).ShouldBeFalse();
    }

    [Fact]
    public void Collection_should_have_different_hash_code_compared_to_null()
    {
        var list1 = new[] { 1 };
        var list2 = default(IEnumerable<int>);

        list1.GetCollectionHashCode().ShouldNotBe(list2.GetCollectionHashCode());
    }

    [Fact]
    public void Null_should_not_be_equal_to_collection()
    {
        var list1 = default(IEnumerable<int>);
        var list2 = new[] { 1 };

        list1.CollectionEquals(list2).ShouldBeFalse();
    }

    [Fact]
    public void Null_should_have_different_hash_code_compared_to_collection()
    {
        var list1 = default(IEnumerable<int>);
        var list2 = new[] { 1 };

        list1.GetCollectionHashCode().ShouldNotBe(list2.GetCollectionHashCode());
    }

    [Fact]
    public void Empty_collection_should_not_be_equal_to_collection()
    {
        var list1 = new int[0];
        var list2 = new[] { 1 };

        list1.CollectionEquals(list2).ShouldBeFalse();
    }

    [Fact]
    public void Empty_collection_should_have_different_hash_code_compared_to_collection()
    {
        var list1 = new int[0];
        var list2 = new[] { 1 };

        list1.GetCollectionHashCode().ShouldNotBe(list2.GetCollectionHashCode());
    }

    [Fact]
    public void Collection_with_different_number_of_elements_should_not_be_equal()
    {
        var list1 = new int?[] { 1, null, 3 };
        var list2 = new int?[] { 1, null, 3, 3 };

        list1.CollectionEquals(list2).ShouldBeFalse();
    }

    [Fact]
    public void Collection_with_different_number_of_elements_should_have_different_hash_code()
    {
        var list1 = new int?[] { 1, null, 3 };
        var list2 = new int?[] { 1, null, 3, 3 };

        list1.GetCollectionHashCode().ShouldNotBe(list2.GetCollectionHashCode());
    }

    [Fact]
    public void Collection_with_different_number_of_null_elements_should_not_be_equal()
    {
        var list1 = new int?[] { 1, null, 3 };
        var list2 = new int?[] { 1, null, 3, null };

        list1.CollectionEquals(list2).ShouldBeFalse();
    }

    [Fact]
    public void Collection_with_different_number_of_null_elements_should_have_different_hash_code()
    {
        var list1 = new int?[] { 1, null, 3 };
        var list2 = new int?[] { 1, null, 3, null };

        list1.GetCollectionHashCode().ShouldNotBe(list2.GetCollectionHashCode());
    }
}
