// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.PropertyInfo;

using Aqua.TypeSystem;
using Shouldly;
using System;
using Xunit;
using BindingFlags = System.Reflection.BindingFlags;

public class When_creating_property_info
{
    private class A
    {
        public static string StaticProperty { get; }

        public string Property { get; }
    }

    [Fact]
    public void Should_throw_on_creating_by_memberinfo_with_null_parameter()
    {
        Should.Throw<ArgumentNullException>(() => new PropertyInfo((System.Reflection.PropertyInfo)null));
    }

    [Fact]
    public void Should_have_set_is_static_for_static_property_info_created_by_memberinfo()
    {
        var property = typeof(A).GetProperty(nameof(A.StaticProperty), BindingFlags.Static | BindingFlags.Public);
        new PropertyInfo(property).IsStatic.ShouldBe(true);
    }

    [Fact]
    public void Should_not_have_set_is_static_by_default_when_created_by_memberinfo()
    {
        var property = typeof(A).GetProperty(nameof(A.Property));
        new PropertyInfo(property).IsStatic.ShouldBeNull();
    }

    [Fact]
    public void Should_not_have_set_is_static_by_default_when_created_by_name()
    {
        new PropertyInfo(nameof(A.Property), typeof(string), typeof(A)).IsStatic.ShouldBeNull();
    }
}
