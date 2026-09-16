// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.EnumerableExtensions;

using Aqua.EnumerableExtensions;

public class When_full_outer_joining_same_type_collections
{
    [Fact]
    public void Should_include_matched_and_unmatched_items()
    {
        new[] { 1, 2 }.FullOuterJoin(new[] { 2, 3 }, x => x).ShouldBe([(1, 0), (2, 2), (0, 3)]);
    }
}

public class When_full_outer_joining_same_type_collections_with_a_result_selector
{
    [Fact]
    public void Should_project_matched_and_unmatched_items()
    {
        new[] { 1, 2 }.FullOuterJoin(new[] { 2, 3 }, x => x, (left, right) => $"{left}:{right}").ShouldBe(["1:0", "2:2", "0:3"]);
    }
}

public class When_full_outer_joining_different_type_collections
{
    [Fact]
    public void Should_use_the_supplied_key_selectors()
    {
        new[] { "a", "bb" }.FullOuterJoin(new[] { 1, 3 }, left => left.Length, right => right).ShouldBe([("a", 1), ("bb", 0), (null, 3)]);
    }
}

public class When_full_outer_joining_different_type_collections_with_comparers
{
    [Fact]
    public void Should_apply_key_and_result_comparers()
    {
        var result = new[] { "One" }.FullOuterJoin(
            new[] { "one" },
            left => left,
            right => right,
            (left, right) => $"{left}:{right}".ToLowerInvariant(),
            StringComparer.OrdinalIgnoreCase,
            StringComparer.OrdinalIgnoreCase);

        result.ShouldBe(["one:one"]);
    }
}

public class When_left_outer_joining_same_type_collections
{
    [Fact]
    public void Should_include_each_left_item()
    {
        new[] { 1, 2 }.LeftOuterJoin(new[] { 2, 3 }, x => x).ShouldBe([(1, 0), (2, 2)]);
    }
}

public class When_left_outer_joining_same_type_collections_with_a_result_selector
{
    [Fact]
    public void Should_project_each_left_item()
    {
        new[] { 1, 2 }.LeftOuterJoin(new[] { 2, 3 }, x => x, (left, right) => (left, right)).ShouldBe([(1, 0), (2, 2)]);
    }
}

public class When_left_outer_joining_different_type_collections
{
    [Fact]
    public void Should_include_unmatched_left_items()
    {
        new[] { "a", "bb" }.LeftOuterJoin(new[] { 1 }, left => left.Length, right => right).ShouldBe([("a", 1), ("bb", 0)]);
    }
}

public class When_left_outer_joining_different_type_collections_with_a_result_selector
{
    [Fact]
    public void Should_use_the_key_comparer()
    {
        new[] { "One" }.LeftOuterJoin(new[] { "one" }, left => left, right => right, (left, right) => right is null ? left : $"{left}/{right}", StringComparer.OrdinalIgnoreCase).ShouldBe(["One/one"]);
    }
}

public class When_right_outer_joining_same_type_collections
{
    [Fact]
    public void Should_include_each_right_item()
    {
        new[] { 1, 2 }.RightOuterJoin(new[] { 2, 3 }, x => x).ShouldBe([(2, 2), (0, 3)]);
    }
}

public class When_right_outer_joining_same_type_collections_with_a_result_selector
{
    [Fact]
    public void Should_project_each_right_item()
    {
        new[] { 1, 2 }.RightOuterJoin(new[] { 2, 3 }, x => x, (left, right) => (left, right)).ShouldBe([(2, 2), (0, 3)]);
    }
}

public class When_right_outer_joining_different_type_collections
{
    [Fact]
    public void Should_include_unmatched_right_items()
    {
        new[] { "a" }.RightOuterJoin(new[] { 1, 2 }, left => left.Length, right => right).ShouldBe([("a", 1), (null, 2)]);
    }
}

public class When_right_outer_joining_different_type_collections_with_a_result_selector
{
    [Fact]
    public void Should_use_the_key_comparer()
    {
        new[] { "One" }.RightOuterJoin(new[] { "one" }, left => left, right => right, (left, right) => left is null ? right : $"{left}/{right}", StringComparer.OrdinalIgnoreCase).ShouldBe(["One/one"]);
    }
}
