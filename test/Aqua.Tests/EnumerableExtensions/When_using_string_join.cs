// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.EnumerableExtensions;

using Aqua.EnumerableExtensions;

public class When_joining_strings_with_a_string_separator
{
    [Fact]
    public void Should_use_the_separator()
    {
        new[] { "one", "two" }.StringJoin(" | ").ShouldBe("one | two");
    }
}

public class When_joining_strings_with_a_character_separator
{
    [Fact]
    public void Should_use_the_separator()
    {
        new[] { "one", "two" }.StringJoin('|').ShouldBe("one|two");
    }
}

public class When_joining_strings_without_a_separator
{
    [Fact]
    public void Should_concatenate_the_items()
    {
        new[] { "one", "two" }.StringJoin().ShouldBe("onetwo");
    }
}

public class When_joining_a_null_string_collection
{
    [Fact]
    public void Should_return_null()
    {
        IEnumerable<string> source = null;

        source.StringJoin().ShouldBeNull();
        source.StringJoin('|').ShouldBeNull();
    }
}
