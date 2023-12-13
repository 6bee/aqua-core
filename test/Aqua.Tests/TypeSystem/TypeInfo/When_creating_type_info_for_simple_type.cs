﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.TypeInfo;

using Aqua.TypeSystem;
using Shouldly;
using System.Linq;
using Xunit;

public class When_creating_type_info_for_simple_type
{
    private class A
    {
        public int Int32Value { get; set; }

        public string StringValue { get; set; }
    }

    private readonly TypeInfo typeInfo;

    public When_creating_type_info_for_simple_type()
    {
        typeInfo = new TypeInfo(typeof(A));
    }

    [Fact]
    public void Type_info_should_have_is_array_false()
    {
        typeInfo.IsArray.ShouldBeFalse();
    }

    [Fact]
    public void Type_info_should_have_is_generic_false()
    {
        typeInfo.IsGenericType.ShouldBeFalse();
    }

    [Fact]
    public void Type_info_should_have_is_nested_true()
    {
        typeInfo.IsNested.ShouldBeTrue();
    }

    [Fact]
    public void Type_info_name_should_be_class_name()
    {
        typeInfo.Name.ShouldBe("A");
    }

    [Fact]
    public void Type_info_should_have_two_properties()
    {
        typeInfo.Properties.Count.ShouldBe(2);
    }

    [Fact]
    public void Type_info_should_contain_int_property()
    {
        typeInfo.Properties.Select(x => x.Name).ShouldContain("Int32Value");
    }

    [Fact]
    public void Type_info_should_contain_string_property()
    {
        typeInfo.Properties.Select(x => x.Name).ShouldContain("StringValue");
    }
}