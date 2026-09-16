// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.EnumerableExtensions;

using Aqua.EnumerableExtensions;

public class When_iterating_a_generic_collection_with_an_action
{
    [Fact]
    public void Should_invoke_the_action_for_each_item()
    {
        var result = new List<int>();

        new[] { 1, 2 }.ForEach(result.Add);

        result.ShouldBe([1, 2]);
    }
}

public class When_iterating_a_generic_collection_with_an_indexed_action
{
    [Fact]
    public void Should_provide_each_item_index()
    {
        var result = new List<string>();

        new[] { "a", "b" }.ForEach((item, index) => result.Add($"{index}:{item}"));

        result.ShouldBe(["0:a", "1:b"]);
    }
}

public class When_iterating_a_generic_collection_with_a_function
{
    [Fact]
    public void Should_invoke_the_function_for_each_item()
    {
        var result = new List<int>();

        new[] { 1, 2 }.ForEach(item => result.Add(item * 2));

        result.ShouldBe([2, 4]);
    }
}

public class When_iterating_a_generic_collection_with_an_indexed_function
{
    [Fact]
    public void Should_provide_each_item_index()
    {
        var result = new List<string>();

        new[] { "a", "b" }.ForEach((item, index) => result.Add($"{index}:{item}"));

        result.ShouldBe(["0:a", "1:b"]);
    }
}

public class When_iterating_a_null_generic_collection
{
    [Fact]
    public void Should_not_invoke_the_callbacks()
    {
        IEnumerable<int> source = null;
        var calls = 0;

        source.ForEach(_ => calls++);
        source.ForEach((_, _) => calls++);
        source.ForEach(_ => { calls++; return 0; });
        source.ForEach((_, _) => { calls++; return 0; });

        calls.ShouldBe(0);
    }
}
