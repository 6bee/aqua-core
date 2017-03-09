// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper
{
    using Aqua.Dynamic;
    using Shouldly;
    using System.Collections.Generic;
    using Xunit;

    public class When_mapping_list_of_string
    {
        DynamicObject dynamicObject;

        public When_mapping_list_of_string()
        {
            dynamicObject = DynamicObject.Create(new List<string> { "One", "Two" });
        }

        [Fact]
        public void Dynamic_object_should_have_one_property_foreach_list_member()
        {
            dynamicObject.PropertyCount.ShouldBe(3);
        }

        [Fact]
        public void Dynamic_object_type_should_be_array_of_string()
        {
            dynamicObject.Type.Type.ShouldBe(typeof(List<string>));
        }

        [Fact]
        public void Dynamic_object_should_have_items_property_with_list()
        {
#if NET
            var itemsPropertyName = "_items";
#else
            var itemsPropertyName = "Items";
#endif
            var items = dynamicObject[itemsPropertyName].ShouldBeOfType<object[]>();
            items[0].ShouldBe("One");
            items[1].ShouldBe("Two");
        }
        
        [Fact]
        public void Dynamic_object_should_result_in_string_array_when_mapped_back()
        {
            var list = dynamicObject.CreateObject().ShouldBeOfType<List<string>>();
            list[0].ShouldBe("One");
            list[1].ShouldBe("Two");
        }
    }
}
