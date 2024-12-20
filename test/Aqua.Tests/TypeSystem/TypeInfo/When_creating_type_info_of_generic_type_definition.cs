// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.TypeInfo;

using Shouldly;
using System.Linq;
using Xunit;
using TypeInfo = Aqua.TypeSystem.TypeInfo;

public class When_creating_type_info_of_generic_type_definition
{
    private class A<T>
    {
        public T Value { get; set; }
    }

    private readonly TypeInfo typeInfo;

    public When_creating_type_info_of_generic_type_definition()
    {
        typeInfo = new TypeInfo(typeof(A<>));
    }

    [Fact]
    public void Type_info_should_have_is_array_false()
    {
        typeInfo.IsArray.ShouldBeFalse();
    }

    [Fact]
    public void Type_info_should_have_is_generic__type_true()
    {
        typeInfo.IsGenericType.ShouldBeTrue();
    }

    [Fact]
    public void Type_info_should_have_is_generic_type_definition_true()
    {
        typeInfo.IsGenericTypeDefinition.ShouldBeTrue();
    }

    [Fact]
    public void Type_info_should_have_is_nested_true()
    {
        typeInfo.IsNested.ShouldBeTrue();
    }

    [Fact]
    public void Type_info_name_should_have_array_brackets()
    {
        typeInfo.Name.ShouldBe("A`1");
    }

    [Fact]
    public void Type_info_should_contain_property()
    {
        typeInfo.Properties.Single().Name.ShouldBe("Value");
        typeInfo.Properties.Single().PropertyType.ToType().IsGenericParameter.ShouldBeTrue();
    }

    [Fact]
    public void Type_info_should_not_contain_any_generic_arguments()
    {
        (typeInfo.GenericArguments?.Count > 0).ShouldBeFalse();
    }
}