// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper
{
    using Aqua.Dynamic;
    using Shouldly;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class When_mapping_from_list
    {
        class CustomType
        {
            public long Int64Property { get; set; }
        }

        IEnumerable<DynamicObject> dynamicObjects;

        public When_mapping_from_list()
        {
            var source = new List<object> { 1, null, "test", new CustomType { Int64Property = 42L } };
            dynamicObjects = new DynamicObjectMapper().MapCollection(source);
        }

        [Fact]
        public void Items_count_should_be_four()
        {
            dynamicObjects.Count().ShouldBe(4);
        }

        [Fact]
        public void First_element_should_be_int32_value()
        {
            var element = (DynamicObject)dynamicObjects.ElementAt(0);
            element.Type.Type.ShouldBe(typeof(int));
            element[""].ShouldBe(1);

            var value = new DynamicObjectMapper().Map(dynamicObjects).Cast<object>().ElementAt(0);
            value.ShouldBe(1);
        }

        [Fact]
        public void Second_element_should_be_null()
        {
            dynamicObjects.ElementAt(1).ShouldBeNull();

            var value = new DynamicObjectMapper().Map(dynamicObjects).Cast<object>().ElementAt(1);
            value.ShouldBeNull();
        }

        [Fact]
        public void Third_element_should_be_string_value()
        {
            var element = (DynamicObject)dynamicObjects.ElementAt(2);
            element.Type.Type.ShouldBe(typeof(string));
            element[""].ShouldBe("test");

            var value = new DynamicObjectMapper().Map(dynamicObjects).Cast<object>().ElementAt(2);
            value.ShouldBe("test");
        }

        [Fact]
        public void Fourth_element_should_be_dynamicobject_for_customtype()
        {
            var element = (DynamicObject)dynamicObjects.ElementAt(3);
            element.Type.Type.ShouldBe(typeof(CustomType));
            element["Int64Property"].ShouldBe(42L);

            var value = new DynamicObjectMapper().Map(dynamicObjects).Cast<object>().ElementAt(3);
            value.ShouldBeOfType<CustomType>();
            ((CustomType)value).Int64Property.ShouldBe(42L);
        }

        //[Fact]
        //public void Fourth_element_should_be_dynamicobject_for_customtype()
        //{
        //    element.Type.Type.ShouldBe(typeof(CustomType));
        //    element["Int64Property"].ShouldBe(42L);
        //}
    }
}
