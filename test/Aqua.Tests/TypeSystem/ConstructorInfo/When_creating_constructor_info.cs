// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.ConstructorInfo;

using Aqua.TypeSystem;
using Shouldly;
using System;
using System.Linq;
using Xunit;
using BindingFlags = System.Reflection.BindingFlags;

public class When_creating_constructor_info
{
    private class A
    {
        static A()
        {
        }

        public A()
        {
        }
    }

    [Fact]
    public void Should_throw_on_creating_by_memberinfo_with_null_parameter()
    {
        Should.Throw<ArgumentNullException>(() => new PropertyInfo((System.Reflection.PropertyInfo)null));
    }

    [Fact]
    public void Should_have_set_is_static_for_type_initializer_created_by_memberinfo()
    {
        var cctor = typeof(A).GetConstructors(BindingFlags.Static | BindingFlags.NonPublic).Single();
        new ConstructorInfo(cctor).IsStatic.ShouldBe(true);
    }

    [Fact]
    public void Should_have_set_is_static_for_type_initializer_created_by_name()
    {
        new ConstructorInfo(".cctor", typeof(A)).IsStatic.ShouldBe(true);
    }

    [Fact]
    public void Should_not_have_set_is_static_by_default_when_created_by_memberinfo()
    {
        var ctor = typeof(A).GetConstructor(Type.EmptyTypes);
        new ConstructorInfo(ctor).IsStatic.ShouldBeNull();
    }

    [Fact]
    public void Should_not_have_set_is_static_by_default_when_created_by_name()
    {
        new ConstructorInfo(".ctor", typeof(A)).IsStatic.ShouldBeNull();
    }
}