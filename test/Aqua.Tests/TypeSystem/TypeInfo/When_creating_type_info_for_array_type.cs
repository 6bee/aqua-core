// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.TypeInfo;

using Aqua.TypeSystem;
using Shouldly;
using Xunit;

public class When_creating_type_info_for_array_type
{
    private class A
    {
    }

    private readonly TypeInfo typeInfo;

    public When_creating_type_info_for_array_type()
    {
        typeInfo = new TypeInfo(typeof(A[]));
    }

    [Fact]
    public void Type_info_should_have_is_array_true()
    {
        typeInfo.IsArray.ShouldBeTrue();
    }

    [Fact]
    public void Type_info_should_have_is_nested_true()
    {
        typeInfo.IsNested.ShouldBeTrue();
    }

    [Fact]
    public void Type_info_should_have_is_generic_false()
    {
        typeInfo.IsGenericType.ShouldBeFalse();
    }

    [Fact]
    public void Type_info_name_should_have_array_brackets()
    {
        typeInfo.Name.ShouldBe("A[]");
    }
}