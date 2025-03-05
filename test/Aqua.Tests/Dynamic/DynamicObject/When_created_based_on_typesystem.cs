// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject;

using Aqua.Dynamic;
using Shouldly;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Xunit;

[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Public fields required by test scenario")]
public class When_created_based_on_typesystem
{
    private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;
    private const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;
    private const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;

    private static int _privateStaticField = 0;
    public static int PublicStaticField = 0;

    private int _privateField = 0;
    public int PublicField = 0;

    private int PrivateProperty { get; set; }

    public int PublicProperty { get; set; }

    private static int PrivateStaticProperty { get; set; }

    public static int PublicStaticProperty { get; set; }

    private int PrivateMethod() => 0;

    public int PublicMethod() => 0;

    private static int PrivateStaticMethod() => 0;

    public static int PublicStaticMethod() => 0;

    [Fact]
    public void Should_map_private_method_info()
    {
        var m = GetType().GetMethod(nameof(PrivateMethod), PrivateInstance);
        var o = new DynamicObject(m);
        var r = new DynamicObjectMapper().Map<MethodInfo>(o);
        r.ShouldBeSameAs(m);
    }

    [Fact]
    public void Should_map_public_method_info()
    {
        var m = GetType().GetMethod(nameof(PublicMethod));
        var o = new DynamicObject(m);
        var r = new DynamicObjectMapper().Map<MethodInfo>(o);
        r.ShouldBeSameAs(m);
    }

    [Fact]
    public void Should_map_private_static_method_info()
    {
        var m = GetType().GetMethod(nameof(PrivateStaticMethod), PrivateStatic);
        var o = new DynamicObject(m);
        var r = new DynamicObjectMapper().Map<MethodInfo>(o);
        r.ShouldBeSameAs(m);
    }

    [Fact]
    public void Should_map_public_static_method_info()
    {
        var m = GetType().GetMethod(nameof(PublicStaticMethod));
        var o = new DynamicObject(m);
        var r = new DynamicObjectMapper().Map<MethodInfo>(o);
        r.ShouldBeSameAs(m);
    }

    [Fact]
    public void Should_map_public_property_info()
    {
        var property = GetType().GetProperty(nameof(PublicProperty));
        var o = new DynamicObject(property);
        var r = new DynamicObjectMapper().Map<PropertyInfo>(o);
        r.ShouldBeSameAs(property);
    }

    [Fact]
    public void Should_map_private_property_info()
    {
        var property = GetType().GetProperty(nameof(PrivateProperty), PrivateInstance);
        var o = new DynamicObject(property);
        var r = new DynamicObjectMapper().Map<PropertyInfo>(o);
        r.ShouldBeSameAs(property);
    }

    [Fact]
    public void Should_map_public_static_property_info()
    {
        var property = GetType().GetProperty(nameof(PublicStaticProperty));
        var o = new DynamicObject(property);
        var r = new DynamicObjectMapper().Map<PropertyInfo>(o);
        r.ShouldBeSameAs(property);
    }

    [Fact]
    public void Should_map_private_static_property_info()
    {
        var property = GetType().GetProperty(nameof(PrivateStaticProperty), PrivateStatic);
        var o = new DynamicObject(property);
        var r = new DynamicObjectMapper().Map<PropertyInfo>(o);
        r.ShouldBeSameAs(property);
    }

    [Fact]
    public void Should_map_private_field_info()
    {
        var field = GetType().GetField(nameof(_privateField), PrivateInstance);
        var o = new DynamicObject(field);
        var r = new DynamicObjectMapper().Map<FieldInfo>(o);
        r.ShouldBeSameAs(field);
    }

    [Fact]
    public void Should_map_public_field_info()
    {
        var field = GetType().GetField(nameof(PublicField));
        var o = new DynamicObject(field);
        var r = new DynamicObjectMapper().Map<FieldInfo>(o);
        r.ShouldBeSameAs(field);
    }

    [Fact]
    public void Should_map_private_static_field_info()
    {
        var field = GetType().GetField(nameof(_privateStaticField), PrivateStatic);
        var o = new DynamicObject(field);
        var r = new DynamicObjectMapper().Map<FieldInfo>(o);
        r.ShouldBeSameAs(field);
    }

    [Fact]
    public void Should_map_public_static_field_info()
    {
        var field = GetType().GetField(nameof(PublicStaticField), PublicStatic);
        var o = new DynamicObject(field);
        var r = new DynamicObjectMapper().Map<FieldInfo>(o);
        r.ShouldBeSameAs(field);
    }

    [Fact]
    public void Should_map_constructor_info()
    {
        var ctor = GetType().GetConstructor(Type.EmptyTypes);
        var o = new DynamicObject(ctor);
        var r = new DynamicObjectMapper().Map<ConstructorInfo>(o);
        r.ShouldBeSameAs(ctor);
    }

    [Fact]
    public void Should_map_type_info()
    {
        var type = GetType().GetTypeInfo();
        var o = new DynamicObjectMapper().MapObject(type);
        var r1 = new DynamicObjectMapper().Map<TypeInfo>(o);
        var r2 = new DynamicObjectMapper().Map<Type>(o);

        r1.ShouldBeSameAs(type);
        r2.ShouldBeSameAs(type.AsType());
    }
}