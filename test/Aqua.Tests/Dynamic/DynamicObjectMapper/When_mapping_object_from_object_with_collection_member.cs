// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper
{
    using Aqua.Dynamic;
    using Shouldly;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class When_mapping_object_from_object_with_collection_member
    {
        class CustomClass
        {
            public IEnumerable<object> Items { get; set; }
        }

        DynamicObject dynamicObject;

        public When_mapping_object_from_object_with_collection_member()
        {
            var source = new CustomClass { Items = new object[] { 1, null, "test" } };
            dynamicObject = new DynamicObjectMapper().MapObject(source);
        }

        [Fact]
        public void Dynamic_object_items_count_should_be_three()
        {
            ((IEnumerable<object>)dynamicObject["Items"]).Count().ShouldBe(3);
        }

        [Fact]
        public void Object_items_count_should_be_three()
        {
            var obj = new DynamicObjectMapper().Map<CustomClass>(dynamicObject);
            obj.Items.Count().ShouldBe(3);
        }
    }
}
