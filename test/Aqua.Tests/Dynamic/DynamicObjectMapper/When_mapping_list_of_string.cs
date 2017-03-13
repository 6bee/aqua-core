// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper
{
    using Aqua.Dynamic;
    using Shouldly;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class When_mapping_list_of_string
    {
        DynamicObject dynamicObject;

        public When_mapping_list_of_string()
        {
            dynamicObject = DynamicObject.Create(new List<string> { "One", "Two" });
        }

        [Fact]
        public void Dynamic_object_should_have_one_property_with_empty_name()
        {
            dynamicObject.Properties.Single().Name.ShouldBe("");
        }

        [Fact]
        public void Dynamic_object_type_should_be_array_of_string()
        {
            dynamicObject.Type.Type.ShouldBe(typeof(List<string>));
        }

        [Fact]
        public void Dynamic_object_should_have_items_property_with_object_array()
        {
            var items = dynamicObject.Values.Single().ShouldBeOfType<object[]>();
            items[0].ShouldBe("One");
            items[1].ShouldBe("Two");
        }
        
        [Fact]
        public void Dynamic_object_should_result_in_list_of_string_when_mapped_back()
        {
            var list = dynamicObject.CreateObject().ShouldBeOfType<List<string>>();
            list[0].ShouldBe("One");
            list[1].ShouldBe("Two");
        }
    }
}
