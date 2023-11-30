// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.TypeResolver;

using Aqua.TypeSystem;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

public class When_resolving_ienumerable_of_anonymous_type
{
    private const BindingFlags AnyStatic = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;

    private static void SayHello<T>(IEnumerable<T> param)
    {
        // empty
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ResolveType_should_return_original_anonymous_type(bool dirtyCache)
    {
        var typeResolver = new TypeResolver();

        Type type1 = TestObjects1.Helper.GetAnonymousType1();
        Type type2 = TestObjects2.Helper.GetAnonymousType1();

        // "Pollute" the type cache
        if (dirtyCache)
        {
            typeResolver.ResolveType(new Aqua.TypeSystem.TypeInfo(type2));
        }

        var resolvedType = typeResolver.ResolveType(new Aqua.TypeSystem.TypeInfo(type1));

        type1.ShouldBe(resolvedType);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ResolveType_should_return_original_ienmuerable_type(bool dirtyCache)
    {
        var typeResolver = new TypeResolver();

        Type type1 = TestObjects1.Helper.GetAnonymousType1();
        Type type2 = TestObjects2.Helper.GetAnonymousType1();

        // "Pollute" the type cache
        if (dirtyCache)
        {
            typeResolver.ResolveType(
                new Aqua.TypeSystem.TypeInfo(typeof(IEnumerable<>).MakeGenericType(type2)));
        }

        var type = typeof(IEnumerable<>).MakeGenericType(type1);

        var resolvedType = typeResolver.ResolveType(new Aqua.TypeSystem.TypeInfo(type));

        type.ShouldBe(resolvedType);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ResolveMethod_should_return_original_method_info(bool dirtyCache)
    {
        var typeResolver = new TypeResolver();

        Type type1 = TestObjects1.Helper.GetAnonymousType1();
        Type type2 = TestObjects2.Helper.GetAnonymousType1();

        // "Pollute" the type cache
        if (dirtyCache)
        {
            typeResolver.ResolveType(
                new Aqua.TypeSystem.TypeInfo(typeof(IEnumerable<>).MakeGenericType(type2)));
        }

        System.Reflection.MethodInfo methodInfo = GetType()
            .GetMethod(nameof(SayHello), AnyStatic)
            .MakeGenericMethod(type1);

        System.Reflection.MethodInfo resolvedMethod =
            new Aqua.TypeSystem.MethodInfo(methodInfo).ResolveMethod(typeResolver);

        resolvedMethod.ShouldBe(methodInfo);
    }
}
