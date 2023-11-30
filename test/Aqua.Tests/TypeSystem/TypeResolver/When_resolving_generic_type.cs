// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.TypeResolver;

using Aqua.TypeSystem;
using Shouldly;
using System;
using System.Collections.Generic;
using Xunit;

public class When_resolving_generic_type
{
    private class A<T>
    {
    }

    private class B
    {
    }

    private readonly Type type;

    public When_resolving_generic_type()
    {
        var typeInfo = new TypeInfo(typeof(IEnumerable<A<B>>));

        type = TypeResolver.Instance.ResolveType(typeInfo);
    }

    [Fact]
    public void Type_should_be_expected_array_type()
    {
        type.ShouldBe(typeof(IEnumerable<A<B>>));
    }
}
