// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject;

using Aqua.Dynamic;
using Shouldly;
using System.Linq;
using Xunit;

public class When_created_based_on_custom_value_type
{
    private struct CustomValue
    {
        public int Id { get; set; }
    }

    private readonly CustomValue source;
    private readonly DynamicObject dynamicObject;

    public When_created_based_on_custom_value_type()
    {
        source = new CustomValue { Id = 123 };
        dynamicObject = new DynamicObject(source);
    }

    [Fact]
    public void Type_property_should_be_set_to_custom_struct()
    {
        dynamicObject.Type.ToType().ShouldBe(typeof(CustomValue));
    }

    [Fact]
    public void Should_have_a_single_member()
    {
        dynamicObject.PropertyCount.ShouldBe(1);
    }

    [Fact]
    public void Member_name_should_be_name_of_property()
    {
        dynamicObject.PropertyNames.Single().ShouldBe("Id");
    }

    [Fact]
    public void Member_value_should_be_property_value()
    {
        dynamicObject["Id"].ShouldBe(source.Id);
    }
}