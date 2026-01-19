// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.TypeResolver;

using Aqua.TypeSystem;
using Shouldly;
using Xunit;

public class When_resolving_type
{
    private class A;

    [Fact]
    public void Resolved_type_should_be_original()
    {
        var typeInfo = new TypeInfo(typeof(A));

        var type = TypeResolver.Instance.ResolveType(typeInfo);

        type.ShouldBe(typeof(A));
    }
}