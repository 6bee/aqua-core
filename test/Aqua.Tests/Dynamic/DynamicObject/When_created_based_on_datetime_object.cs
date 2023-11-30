// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject;

using Aqua.Dynamic;
using Shouldly;
using System;
using System.Linq;
using Xunit;

public class When_created_based_on_datetime_object
{
    private readonly DateTime value;
    private readonly DynamicObject dynamicObject;

    public When_created_based_on_datetime_object()
    {
        value = DateTime.Now;
        dynamicObject = new DynamicObject(value);
    }

    [Fact]
    public void Type_property_should_be_set_to_datetime()
    {
        dynamicObject.Type.ToType().ShouldBe(typeof(DateTime));
    }

    [Fact]
    public void Should_have_a_single_member()
    {
        dynamicObject.PropertyCount.ShouldBe(1);
    }

    [Fact]
    public void Member_name_should_be_empty_string()
    {
        dynamicObject.PropertyNames.Single().ShouldBe(string.Empty);
    }

    [Fact]
    public void Member_value_should_be_initial_value()
    {
        dynamicObject[string.Empty].ShouldBe(value);
    }
}
