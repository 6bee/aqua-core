// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.EnumerableExtensions;

using System.Collections;
using System.Collections.Immutable;
using Aqua.EnumerableExtensions;

public class When_detecting_a_non_string_collection
{
    [Fact]
    public void Should_return_the_collection()
    {
        object value = new[] { 1, 2 };

        value.IsCollection(out var collection).ShouldBeTrue();
        collection.Cast<int>().ShouldBe([1, 2]);
    }
}

public class When_detecting_a_string_as_a_collection
{
    [Fact]
    public void Should_return_false()
    {
        "text".IsCollection(out var collection).ShouldBeFalse();
        collection.ShouldBeNull();
    }
}

public class When_detecting_a_non_collection
{
    [Fact]
    public void Should_return_false()
    {
        1.IsCollection(out var collection).ShouldBeFalse();
        collection.ShouldBeNull();
    }
}

public class When_casting_a_collection_to_an_array_type
{
    [Fact]
    public void Should_return_an_array_of_the_requested_type()
    {
        IEnumerable values = new ArrayList { "1", "2" };

        var result = values.CastCollectionToArrayOfType(typeof(string));

        result.GetType().ShouldBe(typeof(string[]));
        result.Cast<string>().ShouldBe(["1", "2"]);
    }
}

public class When_casting_a_collection_to_a_list_type
{
    [Fact]
    public void Should_return_a_list_of_the_requested_type()
    {
        IEnumerable values = new ArrayList { 1, 2 };

        var result = values.CastCollectionToListOfType(typeof(int));

        result.GetType().ShouldBe(typeof(List<int>));
        result.Cast<int>().ShouldBe([1, 2]);
    }
}

public class When_replacing_a_null_enumerable_with_an_empty_collection
{
    [Fact]
    public void Should_return_an_empty_enumerable()
    {
        IEnumerable<int> source = null;

        source.AsEmptyIfNull().ShouldBeEmpty();
    }
}

public class When_replacing_a_null_read_only_collection_with_an_empty_collection
{
    [Fact]
    public void Should_return_an_empty_read_only_collection()
    {
        IReadOnlyCollection<int> source = null;

        source.AsEmptyIfNull().Count.ShouldBe(0);
    }
}

public class When_replacing_a_null_read_only_list_with_an_empty_collection
{
    [Fact]
    public void Should_return_an_empty_read_only_list()
    {
        IReadOnlyList<int> source = null;

        source.AsEmptyIfNull().Count.ShouldBe(0);
    }
}

public class When_replacing_a_null_list_with_an_empty_collection
{
    [Fact]
    public void Should_return_a_mutable_empty_list()
    {
        List<int> source = null;

        source.AsEmptyIfNull().ShouldBeEmpty();
    }
}

public class When_replacing_a_null_array_with_an_empty_array
{
    [Fact]
    public void Should_return_an_empty_array()
    {
        int[] source = null;

        source.AsEmptyIfNull().ShouldBeEmpty();
    }
}

public class When_replacing_a_null_custom_collection_with_an_empty_instance
{
    private class Collection<T> : IEnumerable<T>
    {
        public IEnumerator<T> GetEnumerator()
        {
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [Fact]
    public void Should_return_an_empty_array()
    {
        Collection<int> source = null;

        source.AsEmptyIfNull().ShouldBeEmpty();
    }
}

public class When_replacing_a_null_immutable_list_with_an_empty_immutable_list
{
    [Fact]
    public void Should_return_an_empty_array()
    {
        ImmutableList<int> source = null;

        source.AsEmptyIfNull(ImmutableList.Create<int>).ShouldBeOfType<ImmutableList<int>>().ShouldBeEmpty();
        source.AsEmptyIfNull(static () => ImmutableList<int>.Empty).ShouldBeOfType<ImmutableList<int>>().ShouldBeEmpty();
        source.AsEmptyIfNull(static () => []).ShouldBeOfType<ImmutableList<int>>().ShouldBeEmpty();
    }
}

public class When_replacing_a_null_iimmutable_list_with_an_empty_immutable_array
{
    [Fact]
    public void Should_return_an_empty_array()
    {
        IImmutableList<int> source = null;

        source.AsEmptyIfNull(static () => ImmutableArray.Create<int>()).ShouldBeAssignableTo<IImmutableList<int>>().ShouldBeEmpty();
        source.AsEmptyIfNull(static () => ImmutableArray<int>.Empty).ShouldBeAssignableTo<IImmutableList<int>>().ShouldBeEmpty();
        source.AsEmptyIfNull(static () => []).ShouldBeAssignableTo<IImmutableList<int>>().ShouldBeEmpty();
    }
}

public class When_replacing_a_null_immutable_dictionary_with_an_empty_immutable_dictionary
{
    [Fact]
    public void Should_return_an_empty_array()
    {
        ImmutableDictionary<int, string> source = null;

        source.AsEmptyIfNull(ImmutableDictionary.Create<int, string>).ShouldBeOfType<ImmutableDictionary<int, string>>().ShouldBeEmpty();
        source.AsEmptyIfNull(static () => ImmutableDictionary<int, string>.Empty).ShouldBeOfType<ImmutableDictionary<int, string>>().ShouldBeEmpty();
    }
}

public class When_replacing_a_null_string_with_an_empty_string
{
    [Fact]
    public void Should_return_an_empty_string()
    {
        string source = null;

        source.AsEmptyIfNull().ShouldBe(string.Empty);
    }
}

public class When_converting_an_empty_enumerable_to_null
{
    [Fact]
    public void Should_return_null()
    {
        Array.Empty<int>().AsNullIfEmpty().ShouldBeNull();
    }
}

public class When_converting_an_empty_string_to_null
{
    [Fact]
    public void Should_return_null()
    {
        string.Empty.AsNullIfEmpty().ShouldBeNull();
    }
}

public class When_converting_a_nonempty_enumerable_to_null
{
    [Fact]
    public void Should_return_the_original_collection()
    {
        var source = new[] { 1 };

        source.AsNullIfEmpty().ShouldBeSameAs(source);
    }
}

public class When_converting_whitespace_to_null
{
    [Fact]
    public void Should_return_null()
    {
        " \t".AsNullIfEmptyOrWhiteSpace().ShouldBeNull();
    }
}

public class When_converting_non_whitespace_to_null
{
    [Fact]
    public void Should_return_the_original_string()
    {
        "value".AsNullIfEmptyOrWhiteSpace().ShouldBe("value");
    }
}

public class When_checking_an_empty_enumerable_for_content
{
    [Fact]
    public void Should_identify_it_as_null_or_empty()
    {
        IEnumerable source = Array.Empty<int>();

        source.IsNullOrEmpty().ShouldBeTrue();
        source.IsNotNullOrEmpty().ShouldBeFalse();
    }
}

public class When_checking_a_nonempty_string_for_content
{
    [Fact]
    public void Should_identify_it_as_not_null_or_empty()
    {
        IEnumerable source = "value";

        source.IsNotNullOrEmpty().ShouldBeTrue();
        source.IsNullOrEmpty().ShouldBeFalse();
    }
}

public class When_checking_a_null_enumerable_for_content
{
    [Fact]
    public void Should_identify_it_as_null_or_empty()
    {
        IEnumerable source = null;

        source.IsNullOrEmpty().ShouldBeTrue();
        source.IsNotNullOrEmpty().ShouldBeFalse();
    }
}

public class When_checking_empty_string_for_content
{
    [Fact]
    public void Should_identify_it_as_not_null_or_empty_but_null_or_whitespace()
    {
        string.Empty.IsNotNullOrEmpty().ShouldBeFalse();
        string.Empty.IsNotNullOrWhiteSpace().ShouldBeFalse();

        string.Empty.IsNullOrEmpty().ShouldBeTrue();
        string.Empty.IsNullOrWhiteSpace().ShouldBeTrue();
    }
}

public class When_checking_whitespace_for_content
{
    [Fact]
    public void Should_identify_it_as_not_null_or_empty_but_null_or_whitespace()
    {
        " ".IsNotNullOrEmpty().ShouldBeTrue();
        " ".IsNotNullOrWhiteSpace().ShouldBeFalse();

        " ".IsNullOrEmpty().ShouldBeFalse();
        " ".IsNullOrWhiteSpace().ShouldBeTrue();
    }
}

public class When_checking_non_whitespace_for_content
{
    [Fact]
    public void Should_identify_it_as_not_null_or_whitespace()
    {
        "value".IsNotNullOrEmpty().ShouldBeTrue();
        "value".IsNotNullOrWhiteSpace().ShouldBeTrue();

        "value".IsNullOrEmpty().ShouldBeFalse();
        "value".IsNullOrWhiteSpace().ShouldBeFalse();
    }
}
