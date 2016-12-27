// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using System;
    using System.Linq;
    using Xunit;
    using Shouldly;

    public class When_created_based_on_guid
    {
        Guid guid;
        DynamicObject dynamicObject;

        public When_created_based_on_guid()
        {
            guid = Guid.NewGuid();

            dynamicObject = new DynamicObject(guid);
        }

        [Fact]
        public void Type_property_should_be_set_to_type_of_guid()
        {
            dynamicObject.Type.Type.ShouldBe(typeof(Guid));
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
        public void Dynamic_guid_properties_should_be_of_type_string()
        {
            dynamicObject.Values.Single().ShouldBeOfType<Guid>();
        }

        [Fact]
        public void Dynamic_guid_properties_should_be_string_represenation_of_guid_values()
        {
            dynamicObject[string.Empty].ShouldBe(guid);
        }
    }
}
