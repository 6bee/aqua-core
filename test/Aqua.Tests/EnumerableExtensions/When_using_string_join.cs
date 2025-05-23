// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.EnumerableExtensions;

using Aqua.EnumerableExtensions;
using Shouldly;
using Xunit;

public class When_using_string_join
{
    [Fact]
    public void Should_allow_string_separator()
    {
        string[] parts = ["one", "two"];
        parts.StringJoin(" | ").ShouldBe("one | two");
    }

    [Fact]
    public void Should_allow_char_separator()
    {
        string[] parts = ["one", "two"];
        parts.StringJoin('|').ShouldBe("one|two");
    }

    [Fact]
    public void Should_allow_no_separator()
    {
        string[] parts = ["one", "two"];
        parts.StringJoin().ShouldBe("onetwo");
    }

    [Fact]
    public void Should_allow_null_string()
    {
        string[] parts = null;
        parts.StringJoin().ShouldBeNull();
    }
}