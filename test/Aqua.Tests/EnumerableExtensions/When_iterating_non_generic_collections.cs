// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.EnumerableExtensions;

using System.Collections;
using Aqua.EnumerableExtensions;

public class When_iterating_a_non_generic_collection_with_an_action
{
    [Fact]
    public void Should_invoke_the_action_for_each_item()
    {
        IEnumerable source = new ArrayList { 1, 2 };
        var result = new List<int>();

        source.ForEach(item => result.Add((int)item));

        result.ShouldBe([1, 2]);
    }
}

public class When_iterating_a_non_generic_collection_with_an_indexed_action
{
    [Fact]
    public void Should_provide_each_item_index()
    {
        IEnumerable source = new ArrayList { "a", "b" };
        var result = new List<string>();

        source.ForEach((item, index) => result.Add($"{index}:{item}"));

        result.ShouldBe(["0:a", "1:b"]);
    }
}

public class When_iterating_a_non_generic_collection_with_a_function
{
    [Fact]
    public void Should_invoke_the_function_for_each_item()
    {
        IEnumerable source = new ArrayList { 1, 2 };
        var result = new List<int>();

        source.ForEach(item => result.Add((int)item * 2));

        result.ShouldBe([2, 4]);
    }
}

public class When_iterating_a_non_generic_collection_with_an_indexed_function
{
    [Fact]
    public void Should_provide_each_item_index()
    {
        IEnumerable source = new ArrayList { "a", "b" };
        var result = new List<string>();

        source.ForEach((item, index) => result.Add($"{index}:{item}"));

        result.ShouldBe(["0:a", "1:b"]);
    }
}

public class When_iterating_a_null_non_generic_collection
{
    [Fact]
    public void Should_not_invoke_the_callbacks()
    {
        IEnumerable source = null;
        var calls = 0;

        source.ForEach(_ => calls++);
        source.ForEach((_, _) => calls++);
        source.ForEach(_ => { calls++; return 0; });
        source.ForEach((_, _) => { calls++; return 0; });

        calls.ShouldBe(0);
    }
}

public class When_checking_a_non_generic_collection_for_any_item
{
    [Fact]
    public void Should_return_true_when_an_item_exists()
    {
        IEnumerable source = new ArrayList { 1, 2 };

        source.Any().ShouldBeTrue();
        source.Any(item => (int)item == 2).ShouldBeTrue();
    }
}

public class When_checking_a_non_generic_collection_without_a_matching_item
{
    [Fact]
    public void Should_return_false()
    {
        IEnumerable source = new ArrayList { 1, 2 };

        source.Any(item => (int)item == 3).ShouldBeFalse();
    }
}

public class When_checking_an_empty_non_generic_collection_for_all_items
{
    [Fact]
    public void Should_return_true()
    {
        IEnumerable source = new ArrayList();

        source.All(_ => false).ShouldBeTrue();
    }
}

public class When_checking_a_non_generic_collection_with_a_nonmatching_item_for_all
{
    [Fact]
    public void Should_return_false()
    {
        IEnumerable source = new ArrayList { 1, 2 };

        source.All(item => (int)item == 1).ShouldBeFalse();
    }
}

public class When_counting_a_non_generic_collection
{
    [Fact]
    public void Should_count_all_and_matching_items()
    {
        IEnumerable source = new ArrayList { 1, 2, 3 };

        source.Count().ShouldBe(3);
        source.Count(item => (int)item % 2 == 1).ShouldBe(2);
    }
}

public class When_counting_a_null_non_generic_collection
{
    [Fact]
    public void Should_return_zero()
    {
        IEnumerable source = null;

        source.Count().ShouldBe(0);
        source.Count(_ => true).ShouldBe(0);
    }
}
