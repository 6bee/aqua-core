// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.TypeResolver;

using Aqua.TypeSystem;
using Shouldly;
using System.Diagnostics.CodeAnalysis;
using Xunit;

public class When_resolving_array_type_of_generic_type
{
    [SuppressMessage("Major Code Smell", "S2326:Unused type parameters should be removed", Justification = "Generic type required for testing")]
    private class A<T>;

    private class B;

    private readonly Type type;

    public When_resolving_array_type_of_generic_type()
    {
        var typeInfo = new TypeInfo(typeof(A<B>[]));

        type = TypeResolver.Instance.ResolveType(typeInfo);
    }

    [Fact]
    public void Type_should_be_expected_array_type()
    {
        type.ShouldBe(typeof(A<B>[]));
    }
}