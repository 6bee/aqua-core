// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.FieldInfo;

using Aqua.TypeSystem;
using Shouldly;
using System;
using Xunit;
using BindingFlags = System.Reflection.BindingFlags;

public class When_creating_field_info
{
    private class A
    {
#pragma warning disable CS0169 // The field is never used
#pragma warning disable IDE0051 // Remove unused private members
        private static string staticField;

        private string field;
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore CS0169 // The field is never used
    }

    private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;
    private const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;

    [Fact]
    public void Should_throw_on_creating_by_memberinfo_with_null_parameter()
    {
        Should.Throw<ArgumentNullException>(() => new FieldInfo((System.Reflection.FieldInfo)null));
    }

    [Fact]
    public void Should_have_set_is_static_for_static_field_info_created_by_memberinfo()
    {
        var field = typeof(A).GetField("staticField", PrivateStatic);
        new FieldInfo(field).IsStatic.ShouldBe(true);
    }

    [Fact]
    public void Should_not_have_set_is_static_by_default_when_created_by_memberinfo()
    {
        var field = typeof(A).GetField("field", PrivateInstance);
        new FieldInfo(field).IsStatic.ShouldBeNull();
    }

    [Fact]
    public void Should_not_have_set_is_static_by_default_when_created_by_name()
    {
        new FieldInfo("field", typeof(A)).IsStatic.ShouldBeNull();
    }
}