// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.TypeResolver;

using Aqua.TypeSystem;
using Shouldly;
using Xunit;

public class When_resolving_different_anonymous_types_with_same_name_and_different_order_of_properties
{
    private readonly Type type1;
    private readonly Type type2;

    private readonly TypeInfo typeInfo1;
    private readonly TypeInfo typeInfo2;

    private readonly Type resolvedType1;
    private readonly Type resolvedType2;

    public When_resolving_different_anonymous_types_with_same_name_and_different_order_of_properties()
    {
        type1 = TestObjects1.Helper.GetAnonymousTypeXY();
        type2 = TestObjects2.Helper.GetAnonymousTypeYX();

        typeInfo1 = new TypeInfo(type1);
        typeInfo2 = new TypeInfo(type2);

        var typeResolver = new TypeResolver();
        resolvedType1 = typeResolver.ResolveType(typeInfo1);
        resolvedType2 = typeResolver.ResolveType(typeInfo2);
    }

    [Fact]
    public void Resolved_types_should_not_be_equal()
    {
        resolvedType1.ShouldNotBe(resolvedType2);
    }

    [Fact]
    public void Resolved_types_should_be_equal_to_original_types()
    {
        type1.ShouldBe(resolvedType1);
        type2.ShouldBe(resolvedType2);
    }
}