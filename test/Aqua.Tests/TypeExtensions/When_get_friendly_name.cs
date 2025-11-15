// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeExtensions;

using Aqua.TypeExtensions;
using Shouldly;
using Xunit;

public class When_get_friendly_name
{
    [Theory]
    [InlineData(typeof(int), true, true, "System.Int32")]
    [InlineData(typeof(int), false, true, "Int32")]
    [InlineData(typeof(int), false, false, "Int32")]
    [InlineData(typeof(int), true, false, "System.Int32")]
    [InlineData(typeof(int?), true, true, "System.Nullable`1[System.Int32]")]
    [InlineData(typeof(int?), false, true, "Nullable`1[Int32]")]
    [InlineData(typeof(IQueryable<int>), true, true, "System.Linq.IQueryable`1[System.Int32]")]
    [InlineData(typeof(IQueryable<>), true, true, "System.Linq.IQueryable`1")]
    [InlineData(typeof(Dictionary<,>), true, true, "System.Collections.Generic.Dictionary`2")]
    [InlineData(typeof(Dictionary<string, int>.Enumerator), true, true, "System.Collections.Generic.Dictionary`2+Enumerator[System.String,System.Int32]")]
    [InlineData(typeof(Dictionary<string, int>.Enumerator), true, false, "System.Collections.Generic.Dictionary`2+Enumerator[System.String,System.Int32]")]
    [InlineData(typeof(Dictionary<string, int>.Enumerator), false, true, "Dictionary`2+Enumerator[String,Int32]")]
    [InlineData(typeof(Dictionary<string, int>.Enumerator), false, false, "Enumerator[String,Int32]")]
    [InlineData(typeof(Dictionary<,>.Enumerator), true, true, "System.Collections.Generic.Dictionary`2+Enumerator")]
    public void Should_format_type_friendy_name(Type type, bool includeNamespance, bool includeDeclaringType, string exectedString)
    {
        var text = type.GetFriendlyName(includeNamespance, includeDeclaringType);
        text.ShouldBe(exectedString);
    }
}