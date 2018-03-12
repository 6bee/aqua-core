// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper
{
    using Aqua.Dynamic;
    using Shouldly;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class When_mapping_object_from_array
    {
        private class CustomType
        {
            public long Int64Property { get; set; }
        }

        private readonly DynamicObject dynamicObject;

        public When_mapping_object_from_array()
        {
            var source = new object[] { 1, null, "test", new CustomType { Int64Property = 42L } };
            dynamicObject = new DynamicObjectMapper().MapObject(source);
        }

        [Fact]
        public void Array_lenght_should_be_four()
        {
            var value = dynamicObject.Get();
            value.ShouldBeOfType<object[]>();
            ((object[])value).Length.ShouldBe(4);

            var array = new DynamicObjectMapper().Map<object[]>(dynamicObject);
            array.Length.ShouldBe(4);
        }

        [Fact]
        public void First_element_should_be_int32_value()
        {
            dynamicObject.Get<object[]>()[0].ShouldBe(1);

            var array = new DynamicObjectMapper().Map<object[]>(dynamicObject);
            array[0].ShouldBe(1);
        }

        [Fact]
        public void Second_element_should_be_null()
        {
            dynamicObject.Get<object[]>()[1].ShouldBeNull();

            var array = new DynamicObjectMapper().Map<object[]>(dynamicObject);
            array[1].ShouldBeNull();
        }

        [Fact]
        public void Third_element_should_be_string_value()
        {
            dynamicObject.Get<object[]>()[2].ShouldBe("test");

            var array = new DynamicObjectMapper().Map<object[]>(dynamicObject);
            array[2].ShouldBe("test");
        }

        [Fact]
        public void Fourth_element_should_be_customtype_with_int64_value()
        {
            var value = dynamicObject.Get<object[]>()[3];
            value.ShouldBeOfType<DynamicObject>();
            ((DynamicObject)value)["Int64Property"].ShouldBe(42L);

            var array = new DynamicObjectMapper().Map<object[]>(dynamicObject);
            array[3].ShouldBeOfType<CustomType>();
            ((CustomType)array[3]).Int64Property.ShouldBe(42L);
        }
    }
}
