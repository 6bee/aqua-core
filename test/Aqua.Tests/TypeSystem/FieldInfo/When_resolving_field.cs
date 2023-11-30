// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.FieldInfo;

using Aqua.TypeSystem;
using Shouldly;
using Xunit;
using BindingFlags = System.Reflection.BindingFlags;

public class When_resolving_field
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
    public void Should_throw_upon_casting_field_info_for_inexistent_field()
    {
        var fieldInfo = new FieldInfo("FIELD", typeof(A));
        ShouldThrowOnResolve(fieldInfo);
    }

    [Fact]
    public void Should_resolve_field()
    {
        var fieldInfo = new FieldInfo("field", typeof(A));
        var field = (System.Reflection.FieldInfo)fieldInfo;
        field.ShouldBeSameAs(typeof(A).GetField("field", PrivateInstance));
    }

    [Fact]
    public void Should_resolve_static_field()
    {
        var fieldInfo = new FieldInfo("staticField", typeof(A)) { IsStatic = true };
        var field = (System.Reflection.FieldInfo)fieldInfo;
        field.ShouldBeSameAs(typeof(A).GetField("staticField", PrivateStatic));
    }

    [Fact]
    public void Should_throw_upon_casting_field_info_missing_isstatic_set_to_true_for_static_member()
    {
        var fieldInfo = new FieldInfo("staticField", typeof(A));
        ShouldThrowOnResolve(fieldInfo);
    }

    private static void ShouldThrowOnResolve(FieldInfo fieldInfo)
        => Should.Throw<TypeResolverException>(() =>
        {
            _ = (System.Reflection.FieldInfo)fieldInfo;
        }).Message.ShouldBe("Failed to resolve field, consider using extension method to specify ITypeResolver.");
}
