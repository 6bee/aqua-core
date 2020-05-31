// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if COREFX

namespace Aqua.Tests.ProtoBuf
{
    using Aqua.Dynamic;
    using Aqua.Extensions;
    using Aqua.TypeSystem;
    using Shouldly;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Xunit;
    using static Aqua.Tests.Serialization.ProtobufNetSerializationHelper;

    public class When_serializing_dynamic_types
    {
        [SkippableTheory]
        [MemberData(nameof(TestData.NativeValues), MemberType = typeof(TestData))]
        public void Should_serialize_scalar_property(Type type, object value, CultureInfo culture)
        {
            SkipUnsupportedDataType(type);

            using var cultureContext = culture.CreateContext();

            var property = new Property("p1", value);
            var model = ProtoBufTypeModel.ConfigureAquaTypes(configureDefaultSystemTypes: false)
                .AddDynamicPropertyType(type, addCollectionSupport: false, addNullableSupport: type.IsNullable())
                .Compile();
            var copy = property.Serialize(model);

            copy.Value.ShouldBe(value);
        }

        [SkippableTheory]
        [MemberData(nameof(TestData.NativeValueArrays), MemberType = typeof(TestData))]
        [MemberData(nameof(TestData.NativeValueLists), MemberType = typeof(TestData))]
        public void Should_serialize_collection_property(Type type, object value, CultureInfo culture)
        {
            SkipUnsupportedDataType(type);

            using var cultureContext = culture.CreateContext();

            var property = new Property("p1", value);
            var model = ProtoBufTypeModel.ConfigureAquaTypes(configureDefaultSystemTypes: false)
                .AddDynamicPropertyType(TypeHelper.GetElementType(type), addSingleValueSuppoort: false, addNullableSupport: type.IsNullable())
                .Compile();
            var copy = property.Serialize(model);

            copy.Value.ShouldBe(value);
        }

        [Fact]
        public void Should_serialize_well_known_property_set()
        {
            var propertySet = new PropertySet
            {
                { "p1", "v1" },
                { "p2", null },
                { "p3", 1 },
            };

            var copy = propertySet.Serialize();

            copy["p1"].ShouldBe(propertySet["p1"]);
            copy["p2"].ShouldBe(propertySet["p2"]);
            copy["p3"].ShouldBe(propertySet["p3"]);

            copy["p1"].ShouldBe("v1");
            copy["p2"].ShouldBeNull();
            copy["p3"].ShouldBe(1);
        }

        [SkippableTheory]
        [MemberData(nameof(TestData.NativeValues), MemberType = typeof(TestData))]
        [MemberData(nameof(TestData.NativeValueArrays), MemberType = typeof(TestData))]
        [MemberData(nameof(TestData.NativeValueLists), MemberType = typeof(TestData))]
        public void Should_serialize_property_set(Type type, object value, CultureInfo culture)
        {
            SkipUnsupportedDataType(type);

            using var cultureContext = culture.CreateContext();

            var propertySet = new PropertySet
            {
                { "p1", value },
            };

            var copy = propertySet.Serialize();

            copy["p1"].ShouldBe(value);
        }

        [SkippableTheory]
        [MemberData(nameof(TestData.NativeValues), MemberType = typeof(TestData))]
        public void Should_serialize_dynamic_object(Type type, object value, CultureInfo culture)
        {
            SkipUnsupportedDataType(type);

            using var cultureContext = culture.CreateContext();

            var dynamicObject = new DynamicObjectMapper().MapObject(value);
            var copy = dynamicObject.Serialize();

            copy?.Get().ShouldBe(dynamicObject?.Get(), $"type: {type} value: {value}");

            var c = new DynamicObjectMapper().Map(copy);
            c.ShouldBe(value);
        }

        [SkippableTheory]
        [MemberData(nameof(TestData.NativeValueArrays), MemberType = typeof(TestData))]
        [MemberData(nameof(TestData.NativeValueLists), MemberType = typeof(TestData))]
        public void Should_serialize_dynamic_object_collection(Type type, IEnumerable value, CultureInfo culture)
        {
            SkipUnsupportedDataType(type);
            Skip.If(
                value.Cast<object>().Any(x => x is null),
                "protobuf-net doesn't support serialization of collection with null elements as the root object");

            using var cultureContext = culture.CreateContext();

            var dynamicObjects = new DynamicObjectMapper().MapCollection(value);
            var copy = dynamicObjects.Serialize();

            var dynamicObjectsCount = dynamicObjects?.Count() ?? 0;
            var copyCount = copy?.Count() ?? 0;
            copyCount.ShouldBe(dynamicObjectsCount);

            for (int i = 0; i < copyCount; i++)
            {
                var originalValue = dynamicObjects.ElementAt(i)?.Get();
                var copyValue = copy.ElementAt(i)?.Get();
                copyValue.ShouldBe(originalValue);
            }

            var c = new DynamicObjectMapper().Map(copy);
            if (value is null)
            {
                c.ShouldBeNull();
            }
            else
            {
                var array = value.Cast<object>().ToArray();
                var copyArray = c.Cast<object>().ToArray();

                for (int i = 0; i < copyArray.Length; i++)
                {
                    copyArray[i].ShouldBe(array[i]);
                }
            }
        }

        public class OrderEntity
        {
            public int Id { get; set; }

            public ICollection<OrderItemEntity> Items { get; set; }
        }

        public class OrderItemEntity
        {
            public int ProductId { get; set; }

            public uint Quantity { get; set; }

            public ProductEntity Product { get; set; }
        }

        public class ProductEntity
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public decimal Price { get; set; }
        }

        [Fact]
        public void Should_serialize_dynamic_object_for_entity()
        {
            var entity = new DynamicObjectMapper().MapObject(new ProductEntity
            {
                Id = 1,
                Name = "Product E. Name",
                Price = 1234.56m,
            });

            var copy = entity.Serialize();

            copy["Id"].ShouldBe(1);
            copy["Name"].ShouldBe("Product E. Name");
            copy["Price"].ShouldBe(1234.56m);
        }

        [Fact]
        public void Should_serialize_dynamic_object_for_entity_set()
        {
            var entity = new DynamicObjectMapper().MapCollection(new List<ProductEntity>
            {
                new ProductEntity
                {
                    Id = 1,
                    Name = "Product E. Name",
                    Price = 1234.56m,
                },
                new ProductEntity
                {
                    Id = 2,
                    Name = "Noname",
                    Price = 78.9m,
                },
            });

            var copy = entity.Serialize();

            var item0 = copy.ElementAt(0);
            item0["Id"].ShouldBe(1);
            item0["Name"].ShouldBe("Product E. Name");
            item0["Price"].ShouldBe(1234.56m);

            var item1 = copy.ElementAt(1);
            item1["Id"].ShouldBe(2);
            item1["Name"].ShouldBe("Noname");
            item1["Price"].ShouldBe(78.9m);
        }

        [Fact]
        public void Should_serialize_nested_dynamic_object_for_entity_relation()
        {
            var entity = new DynamicObjectMapper().MapObject(new OrderItemEntity
            {
                ProductId = 1,
                Quantity = 2,
                Product = new ProductEntity
                {
                    Id = 1,
                    Name = "Product E. Name",
                    Price = 1234.56m,
                },
            });

            var copy = entity.Serialize();

            copy["ProductId"].ShouldBe(1);
            copy["Quantity"].ShouldBe(2u);
            var product = copy["Product"].ShouldBeAssignableTo<DynamicObject>();
            product["Id"].ShouldBe(1);
            product["Name"].ShouldBe("Product E. Name");
            product["Price"].ShouldBe(1234.56m);
        }

        [Fact]
        public void Should_serialize_nested_dynamic_object_for_entity_collection()
        {
            var entity = new DynamicObjectMapper().MapObject(new OrderEntity
            {
                Id = 1,
                Items = new List<OrderItemEntity>
                {
                    new OrderItemEntity
                    {
                        ProductId = 11,
                        Quantity = 2,
                        Product = new ProductEntity
                        {
                            Id = 11,
                            Name = "Product E. Name",
                            Price = 1234.56m,
                        },
                    },
                    new OrderItemEntity
                    {
                        ProductId = 22,
                        Quantity = 4,
                        Product = new ProductEntity
                        {
                            Id = 22,
                            Name = "Noname",
                            Price = 78.9m,
                        },
                    },
                },
            });

            var copy = entity.Serialize();

            copy["Id"].ShouldBe(1);
            var items = copy["Items"].ShouldBeAssignableTo<ICollection<DynamicObject>>();
            items.Count.ShouldBe(2);
        }
    }
}

#endif // COREFX
