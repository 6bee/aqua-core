// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using Shouldly;
    using System.Linq;
    using Xunit;

    public class When_created_based_on_custom_type
    {
        private class CustomClass
        {
            public string Prop1 { get; set; }
        }

        private readonly CustomClass source;
        private readonly DynamicObject dynamicObject;

        public When_created_based_on_custom_type()
        {
            source = new CustomClass { Prop1 = "Value1" };
            dynamicObject = new DynamicObject(source);
        }

        [Fact]
        public void Type_property_should_be_set_to_custom_class()
        {
            dynamicObject.Type.Type.ShouldBe(typeof(CustomClass));
        }

        [Fact]
        public void Should_have_a_single_member()
        {
            dynamicObject.PropertyCount.ShouldBe(1);
        }

        [Fact]
        public void Member_name_should_be_name_of_property()
        {
            dynamicObject.PropertyNames.Single().ShouldBe("Prop1");
        }

        [Fact]
        public void Member_value_should_be_property_value()
        {
            dynamicObject["Prop1"].ShouldBe(source.Prop1);
        }
    }
}
