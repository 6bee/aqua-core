// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper
{
    using Aqua.Dynamic;
    using Shouldly;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class When_mapping_list_of_nullable_int
    {
        DynamicObject dynamicObject;

        public When_mapping_list_of_nullable_int()
        {
            IEnumerable<int?> list = new List<int?> { null, 1, 11 };
            dynamicObject = DynamicObject.Create(list);
        }

        [Fact]
        public void Dynamic_object_should_have_one_property_with_empty_name()
        {
            dynamicObject.Properties.Single().Name.ShouldBe("");
        }

        [Fact]
        public void Dynamic_object_type_should_be_array_of_string()
        {
            dynamicObject.Type.Type.ShouldBe(typeof(List<int?>));
        }

        [Fact]
        public void Dynamic_object_should_have_items_property_with_list()
        {
            var items = dynamicObject.Values.Single().ShouldBeOfType<object[]>();
            items[0].ShouldBeNull();
            items[1].ShouldBe(1);
            items[2].ShouldBe(11);
        }
        
        [Fact]
        public void Dynamic_object_should_result_in_string_array_when_mapped_back()
        {
            var list = dynamicObject.CreateObject().ShouldBeOfType<List<int?>>();
            list[0].ShouldBeNull();
            list[1].ShouldBe(1);
            list[2].ShouldBe(11);
        }
    }
}
