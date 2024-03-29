﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.TypeInfo;

using Aqua.TypeSystem;
using Shouldly;
using System.Linq;
using Xunit;

public class When_creating_type_info_of_anonymous_type
{
    private readonly TypeInfo typeInfo;

    public When_creating_type_info_of_anonymous_type()
    {
        var instance = new { Int32Value = 0, StringValue = string.Empty };
        var type = instance.GetType();
        typeInfo = new TypeInfo(type);
    }

    [Fact]
    public void Type_info_should_have_is_anonymous_true()
    {
        typeInfo.IsAnonymousType.ShouldBeTrue();
    }

    [Fact]
    public void Type_info_should_have_is_array_false()
    {
        typeInfo.IsArray.ShouldBeFalse();
    }

    [Fact]
    public void Type_info_should_have_is_generic_true()
    {
        typeInfo.IsGenericType.ShouldBeTrue();
    }

    [Fact]
    public void Type_info_should_have_is_nested_false()
    {
        typeInfo.IsNested.ShouldBeFalse();
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