// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper
{
    using Aqua.Dynamic;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class When_mapping_object_with_collection_property
    {
        private class CustomType<T>
        {
            public IEnumerable<T> Items { get; set; }
        }

        private class Item
        {
        }

        private static CustomType<T> CreateObject<T>(IEnumerable<T> items) => new CustomType<T> { Items = items };

        [Fact]
        public void Should_preserve_int_array_type()
        {
            var obj = CreateObject(new[] { 1, int.MinValue });

            var dynamicObject = new DynamicObjectMapper().MapObject(obj);

            var items = dynamicObject["Items"];

            var array = items.ShouldBeOfType<object[]>();
            array[0].ShouldBe(1);
            array[1].ShouldBe(int.MinValue);
        }

        [Fact]
        public void Should_preserve_nullable_sbyte_array_type()
        {
            var obj = CreateObject(new sbyte?[] { 1, null, -128 });

            var dynamicObject = new DynamicObjectMapper().MapObject(obj);

            var items = dynamicObject["Items"];

            var array = items.ShouldBeOfType<object[]>();
            array[0].ShouldBe((sbyte)1);
            array[1].ShouldBeNull();
            array[2].ShouldBe((sbyte)-128);
        }

        [Fact]
        public void Should_map_nullable_sbyte_list_to_nullable_sbyte_array()
        {
            var obj = CreateObject(new List<sbyte?> { 1, null, -128 });

            var dynamicObject = new DynamicObjectMapper().MapObject(obj);

            var items = dynamicObject["Items"];

            var array = items.ShouldBeOfType<object[]>();
            array[0].ShouldBe((sbyte)1);
            array[1].ShouldBeNull();
            array[2].ShouldBe((sbyte)-128);
        }

        [Fact]
        public void Should_map_nullable_sbyte_array_to_string_array_according_to_mapper_settings()
        {
            var obj = CreateObject(new sbyte?[] { 1, null, -128 });

            var mapperSettings = new DynamicObjectMapperSettings { FormatNativeTypesAsString = true };

            var dynamicObject = new DynamicObjectMapper(mapperSettings).MapObject(obj);

            var items = dynamicObject["Items"];

            var array = items.ShouldBeOfType<object[]>();
            array[0].ShouldBe("1");
            array[1].ShouldBeNull();
            array[2].ShouldBe("-128");
        }

        [Fact]
        public void Should_preserve_empty_nullable_DateTimeOffset_array_type()
        {
            var obj = CreateObject(new DateTimeOffset?[0]);

            var dynamicObject = new DynamicObjectMapper().MapObject(obj);

            dynamicObject["Items"]
                .ShouldBeOfType<object[]>()
                .ShouldBeEmpty();
        }

        [Fact]
        public void Should_map_custom_type_array_to_object_array()
        {
            var obj = CreateObject(new[] { new Item() });

            var dynamicObject = new DynamicObjectMapper().MapObject(obj);

            var items = dynamicObject["Items"];

            var array = items.ShouldBeOfType<object[]>();

            array.Length.ShouldBe(1);

            array[0]
                .ShouldBeOfType<DynamicObject>()
                .Type.Type.ShouldBe(typeof(Item));
        }

        [Fact]
        public void Should_map_custom_type_list_to_object_array()
        {
            var obj = CreateObject(new List<Item> { new Item() });

            var dynamicObject = new DynamicObjectMapper().MapObject(obj);

            var items = dynamicObject["Items"];

            var array = items.ShouldBeOfType<object[]>();

            array.Length.ShouldBe(1);

            array[0]
                .ShouldBeOfType<DynamicObject>()
                .Type.Type.ShouldBe(typeof(Item));
        }

        [Fact]
        public void Should_map_jagged_int_array_to_object_array_containing_int_array()
        {
            var obj = CreateObject(new int[][]
            {
                new[] { 1, 2 },
                new[] { 3 },
                new[] { 4, 5, 6 },
            });

            var dynamicObject = new DynamicObjectMapper().MapObject(obj);

            var items = dynamicObject["Items"];

            var outerArray = items.ShouldBeOfType<object[]>();
            outerArray.Length.ShouldBe(3);

            var innerArray1 = outerArray[0].ShouldBeOfType<object[]>();
            innerArray1.Length.ShouldBe(2);
            innerArray1[0].ShouldBe(1);
            innerArray1[1].ShouldBe(2);

            var innerArray2 = outerArray[1].ShouldBeOfType<object[]>();
            innerArray2.Length.ShouldBe(1);
            innerArray2[0].ShouldBe(3);

            var innerArray3 = outerArray[2].ShouldBeOfType<object[]>();
            innerArray3.Length.ShouldBe(3);
            innerArray3[0].ShouldBe(4);
            innerArray3[1].ShouldBe(5);
            innerArray3[2].ShouldBe(6);
        }

        [Fact]
        public void Should_map_multidimensional_int_array_to_int_array()
        {
#pragma warning disable SA1500 // Braces for multi-line statements should not share line
            var obj = new
            {
                Items = new int[,,]
                {
                    {
                        { 1, 2, 3 },
                        { 4, 5, 6 },
                    },
                    {
                        { 7, 8, 9 },
                        { 10, 11, 12 },
                    },
                    {
                        { 13, 14, 15 },
                        { 16, 17, 18 },
                    },
                },
            };
#pragma warning restore SA1500 // Braces for multi-line statements should not share line

            var dynamicObject = new DynamicObjectMapper().MapObject(obj);

            var items = dynamicObject["Items"];

            var array = items.ShouldBeOfType<object[]>();
            array.SequenceEqual(Enumerable.Range(1, 18).Select(x => (object)x)).ShouldBeTrue();
        }
    }
}
