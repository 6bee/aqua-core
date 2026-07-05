// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.TypeSafety;

using Aqua.Dynamic;
using TypeSafety = Aqua.Dynamic.TypeSafety;

public sealed class When_using_type_safety_helpers
{
    private sealed class AllowedType;

    private sealed class OtherType;

    [Theory]
    [InlineData(typeof(AllowedType))]
    [InlineData(typeof(OtherType))]
    [InlineData(typeof(string))]
    public void AllowAny_should_never_throw(Type type)
    {
        Should.NotThrow(() => TypeSafety.AllowAny.AssertTypeSafety(type));
    }

    [Fact]
    public void AllowAny_should_throw_argument_null_exception_for_null_type()
    {
        Should.Throw<ArgumentNullException>(() => TypeSafety.AllowAny.AssertTypeSafety(null));
    }

    [Theory]
    [InlineData(typeof(AllowedType))]
    [InlineData(typeof(AllowedType[]))]
    public void AllowList_should_not_throw_for_listed_type(Type type)
    {
        var checker = TypeSafety.AllowList(typeof(AllowedType));

        Should.NotThrow(() => checker.AssertTypeSafety(type));
    }

    [Theory]
    [InlineData(typeof(OtherType))]
    [InlineData(typeof(OtherType[]))]
    [InlineData(typeof(string))]
    public void AllowList_should_throw_for_unlisted_type(Type type)
    {
        var checker = TypeSafety.AllowList(typeof(AllowedType));

        Should.Throw<TypeSafetyException>(() => checker.AssertTypeSafety(type));
    }

    [Fact]
    public void AllowList_should_resolve_nullable_element_to_allowed_type()
    {
        var checker = TypeSafety.AllowList(typeof(int));

        Should.NotThrow(() => checker.AssertTypeSafety(typeof(int?)));
        Should.NotThrow(() => checker.AssertTypeSafety(typeof(int[])));
        Should.NotThrow(() => checker.AssertTypeSafety(typeof(int?[])));
    }

    [Fact]
    public void AllowList_with_empty_list_should_throw_for_any_type()
    {
        var checker = TypeSafety.AllowList();

        Should.Throw<TypeSafetyException>(() => checker.AssertTypeSafety(typeof(AllowedType)));
    }

    [Fact]
    public void AllowList_should_throw_argument_null_exception_for_null_type()
    {
        var checker = TypeSafety.AllowList(typeof(AllowedType));

        Should.Throw<ArgumentNullException>(() => checker.AssertTypeSafety(null));
    }

    [Fact]
    public void AllowList_should_throw_argument_null_exception_for_null_allowed_types()
    {
        Should.Throw<ArgumentNullException>(() => TypeSafety.AllowList((IEnumerable<Type>)null));
    }
}
