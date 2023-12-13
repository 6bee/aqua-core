// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.PropertyInfo;

using Aqua.TypeSystem;
using Shouldly;
using Xunit;
using BindingFlags = System.Reflection.BindingFlags;

public class When_resolving_property
{
    private class A
    {
        public static string StaticProperty { get; }

        public string Property { get; }
    }

    [Fact]
    public void Should_throw_upon_casting_property_info_for_inexistent_property()
    {
        var propertyInfo = new PropertyInfo("PROPERTY", typeof(string), typeof(A));
        ShouldThrowOnResolve(propertyInfo);
    }

    [Fact]
    public void Should_resolve_property()
    {
        var propertyInfo = new PropertyInfo(nameof(A.Property), typeof(string), typeof(A));
        var property = (System.Reflection.PropertyInfo)propertyInfo;
        property.ShouldBeSameAs(typeof(A).GetProperty(nameof(A.Property)));
    }

    [Fact]
    public void Should_resolve_static_property()
    {
        var propertyInfo = new PropertyInfo(nameof(A.StaticProperty), typeof(string), typeof(A)) { IsStatic = true };
        var property = (System.Reflection.PropertyInfo)propertyInfo;
        property.ShouldBeSameAs(typeof(A).GetProperty(nameof(A.StaticProperty), BindingFlags.Public | BindingFlags.Static));
    }

    [Fact]
    public void Should_throw_on_resolve_static_property_missing_is_static_set_to_true()
    {
        var propertyInfo = new PropertyInfo(nameof(A.StaticProperty), typeof(string), typeof(A));
        ShouldThrowOnResolve(propertyInfo);
    }

    private static void ShouldThrowOnResolve(PropertyInfo propertyInfo)
        => Should.Throw<TypeResolverException>(() =>
        {
            _ = (System.Reflection.PropertyInfo)propertyInfo;
        }).Message.ShouldBe("Failed to resolve property, consider using extension method to specify ITypeResolver.");
}