// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper
{
    using Aqua.Dynamic;
    using Shouldly;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class When_mapping_list_of_string_as_collection
    {
        private readonly IEnumerable<DynamicObject> dynamicObjects;

        public When_mapping_list_of_string_as_collection()
        {
            dynamicObjects = new DynamicObjectMapper().MapCollection(new List<string> { "One", null });
        }

        [Fact]
        public void Dynamic_objects_collection_should_have_two_items()
        {
            dynamicObjects.Count().ShouldBe(2);
        }

        [Fact]
        public void Dynamic_object_type_should_be_array_of_string()
        {
            dynamicObjects.ShouldAllBe(x => x == null || x.Type.ToType() == typeof(string));
        }

        [Fact]
        public void String_item_should_be_presened_as_dynamic_object()
        {
            var item = dynamicObjects.First();
            item.PropertyNames.Single().ShouldBeEmpty();
            item.Values.Single().ShouldBe("One");
        }

        [Fact]
        public void Null_string_item_should_be_null()
        {
            dynamicObjects.Last().ShouldBeNull();
        }
    }
}
