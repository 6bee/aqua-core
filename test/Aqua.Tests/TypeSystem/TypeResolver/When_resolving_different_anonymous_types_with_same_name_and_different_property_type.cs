// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.TypeResolver;

using Aqua.TypeSystem;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using TypeInfo = Aqua.TypeSystem.TypeInfo;

public class When_resolving_different_anonymous_types_with_same_name_and_different_property_type
{
    private readonly Type type1;
    private readonly Type type2;

    private readonly TypeInfo typeInfo1;
    private readonly TypeInfo typeInfo2;

    private readonly Type resolvedType1;
    private readonly Type resolvedType2;

    public When_resolving_different_anonymous_types_with_same_name_and_different_property_type()
    {
        type1 = TestObjects1.Helper.GetAnonymousType0<int, string>();
        type2 = TestObjects2.Helper.GetAnonymousType0<long, string>();

        typeInfo1 = new TypeInfo(type1);
        typeInfo2 = new TypeInfo(type2);

        var typeResolver = new TypeResolver();
        resolvedType1 = typeResolver.ResolveType(typeInfo1);
        resolvedType2 = typeResolver.ResolveType(typeInfo2);
    }

    [Fact]
    public void Type_infos_should_not_be_equal()
    {
        typeInfo1.ShouldNotBe(typeInfo2);
    }

    [Fact]
    public void Resolved_types_should_not_be_equal()
    {
        resolvedType1.ShouldNotBe(resolvedType2);
    }

    [Fact]
    public void Resolved_types_should_be_equal_to_original_types()
    {
        // may be resolved to anonymous type of more than one assembly
        resolvedType1.FullName.ShouldBe(type1.FullName);
        GetPropertyDescriptions(resolvedType1).SequenceShouldBeEqual(GetPropertyDescriptions(type1));

        resolvedType2.FullName.ShouldBe(type2.FullName);
        GetPropertyDescriptions(resolvedType2).SequenceShouldBeEqual(GetPropertyDescriptions(type2));
    }

    private static IEnumerable<object> GetPropertyDescriptions(Type type)
        => type
        .GetTypeInfo()
        .GetProperties()
        .Select(p => new { p.Name, PropertyType = p.PropertyType.FullName })
        .ToArray();
}
