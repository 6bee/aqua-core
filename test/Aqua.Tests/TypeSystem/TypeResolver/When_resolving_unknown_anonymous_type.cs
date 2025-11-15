// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.TypeResolver;

using Aqua.TypeSystem;
using Aqua.TypeSystem.Emit;
using Shouldly;
using Xunit;
using TypeInfo = Aqua.TypeSystem.TypeInfo;

public class When_resolving_unknown_anonymous_type
{
    private readonly Type actualType;
    private readonly Type emitedType;

    public When_resolving_unknown_anonymous_type()
    {
        var instance = new { Int32Value = 0, StringValue = string.Empty };

        actualType = instance.GetType();

        var typeInfo = new TypeInfo(actualType);

        typeInfo.Name = "UnknowTestClass";
        typeInfo.Namespace = "Unkown.Test.Namespace";

        emitedType = new TypeResolver().ResolveType(typeInfo);
    }

    [Fact]
    public void Type_should_be_different_from_actual_type()
    {
        emitedType.ShouldNotBe(actualType);
    }

    [Fact]
    public void Type_should_be_dynamically_emited_type()
    {
        emitedType.ShouldBeAnnotatedWith<EmittedTypeAttribute>();
    }

    [Fact]
    public void Emited_type_should_be_generic_type()
    {
        emitedType.IsGenericType.ShouldBeTrue();
    }

    [Fact]
    public void Emited_type_should_closed_generic_type()
    {
        emitedType.IsGenericTypeDefinition.ShouldBeFalse();
    }

    [Fact]
    public void Emited_type_should_have_two_generic_arguments()
    {
        emitedType.GetGenericArguments().Length.ShouldBe(2);
    }

    [Fact]
    public void Emited_type_should_have_two_properties()
    {
        emitedType.GetProperties().Length.ShouldBe(2);
    }

    [Fact]
    public void All_properties_should_be_readonly()
    {
        foreach (var p in emitedType.GetProperties())
        {
            p.CanRead.ShouldBeTrue();
            p.CanWrite.ShouldBeFalse();
        }
    }

    [Fact]
    public void Emited_type_should_have_int_property()
    {
        emitedType.GetProperty("Int32Value").PropertyType.ShouldBe(typeof(int));
    }

    [Fact]
    public void Emited_type_should_have_string_property()
    {
        emitedType.GetProperty("StringValue").PropertyType.ShouldBe(typeof(string));
    }
}