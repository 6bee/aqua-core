// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.TypeResolver;

using Aqua.TypeSystem;
using Shouldly;
using System;
using Xunit;

public class When_resolving_a_types_with_two_resolvers
{
    private class A
    {
        public int Int32Value { get; set; }

        public string StringValue { get; set; }
    }

    private readonly Type resolvedType1;
    private readonly Type resolvedType2;

    public When_resolving_a_types_with_two_resolvers()
    {
        var a = new A { Int32Value = 0, StringValue = string.Empty };

        var typeInfo = new TypeInfo(a.GetType());

        typeInfo.Name = "TestTypeName";
        typeInfo.Namespace = "TestNamespace";
        typeInfo.DeclaringType = null;

        resolvedType1 = new TypeResolver().ResolveType(typeInfo);

        resolvedType2 = new TypeResolver().ResolveType(typeInfo);
    }

    [Fact]
    public void Resolved_types_should_be_different()
    {
        resolvedType1.ShouldNotBe(resolvedType2);
    }
}