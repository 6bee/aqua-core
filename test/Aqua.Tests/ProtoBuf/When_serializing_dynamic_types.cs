// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.ProtoBuf;

using Aqua.Dynamic;
using Aqua.ProtoBuf;
using Aqua.TypeExtensions;
using Aqua.TypeSystem;
using Shouldly;
using System.Collections;
using System.Globalization;
using Xunit;
using static Aqua.Tests.Serialization.ProtobufNetSerializationHelper;

public class When_serializing_dynamic_types
{
    public class A
    {
        public A Reference { get; set; }
    }

    [SkippableTheory]
    [MemberData(nameof(TestData.TestValues), MemberType = typeof(TestData))]
    public void Should_serialize_scalar_property(Type type, object value, CultureInfo culture)
    {
        SkipUnsupportedDataType(type, value);

        using var cultureContext = culture.CreateContext();

        var property = new Property("p1", value);
        var model = ProtoBufTypeModel.ConfigureAquaTypes(configureDefaultSystemTypes: false)
            .AddDynamicPropertyType(type, addCollectionSupport: false, addNullableSupport: type.IsNullableType())
            .Compile();
        var copy = property.Clone(model);

        copy.Value.ShouldBe(value);
    }

    [SkippableTheory]
    [MemberData(nameof(TestData.TestValueArrays), MemberType = typeof(TestData))]
    [MemberData(nameof(TestData.TestValueLists), MemberType = typeof(TestData))]
    public void Should_serialize_collection_property(Type type, object value, CultureInfo culture)
    {
        SkipUnsupportedDataType(type, value);

        using var cultureContext = culture.CreateContext();

        var property = new Property("p1", value);
        var model = ProtoBufTypeModel.ConfigureAquaTypes(configureDefaultSystemTypes: false)
            .AddDynamicPropertyType(TypeHelper.GetElementType(type), addSingleValueSuppoort: false, addNullableSupport: type.IsNullableType())
            .Compile();
        var copy = property.Clone(model);

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

        var copy = new DynamicObject(propertySet).Clone();

        copy["p1"].ShouldBe(propertySet["p1"]);
        copy["p2"].ShouldBe(propertySet["p2"]);
        copy["p3"].ShouldBe(propertySet["p3"]);

        copy["p1"].ShouldBe("v1");
        copy["p2"].ShouldBeNull();
        copy["p3"].ShouldBe(1);
    }

    [SkippableTheory]
    [MemberData(nameof(TestData.TestValues), MemberType = typeof(TestData))]
    [MemberData(nameof(TestData.TestValueArrays), MemberType = typeof(TestData))]
    [MemberData(nameof(TestData.TestValueLists), MemberType = typeof(TestData))]
    public void Should_serialize_property_set(Type type, object value, CultureInfo culture)
    {
        SkipUnsupportedDataType(type, value);

        using var cultureContext = culture.CreateContext();

        var propertySet = new PropertySet
        {
            { "p1", value },
        };

        var config = ProtoBufTypeModel.ConfigureAquaTypes(configureDefaultSystemTypes: false)
            .AddDynamicPropertyType(TypeHelper.GetElementType(type) ?? type)
            .Compile();

        var copy = new DynamicObject(propertySet).Clone(config);

        copy["p1"].ShouldBe(value);
    }

    [SkippableTheory]
    [MemberData(nameof(TestData.TestValues), MemberType = typeof(TestData))]
    public void Should_serialize_dynamic_object(Type type, object value, CultureInfo culture)
    {
        SkipUnsupportedDataType(type, value);

        using var cultureContext = culture.CreateContext();

        var dynamicObject = new DynamicObjectMapper().MapObject(value);

        var copy = dynamicObject.Clone();

        copy?.Get().ShouldBe(dynamicObject?.Get(), $"type: {type} value: {value}");

        var c = new DynamicObjectMapper().Map(copy);
        c.ShouldBe(value);
    }

    [SkippableTheory]
    [MemberData(nameof(TestData.TestValueArrays), MemberType = typeof(TestData))]
    [MemberData(nameof(TestData.TestValueLists), MemberType = typeof(TestData))]
    public void Should_serialize_dynamic_object_collection(Type type, IEnumerable value, CultureInfo culture)
    {
        SkipUnsupportedDataType(type, value);

        using var cultureContext = culture.CreateContext();

        var dynamicObjects = new DynamicObjectMapper(new DynamicObjectMapperSettings { WrapNullAsDynamicObject = true }).MapCollection(value);
        var copy = dynamicObjects.Clone();

        var dynamicObjectsCount = dynamicObjects?.Count ?? 0;
        var copyCount = copy?.Count ?? 0;
        copyCount.ShouldBe(dynamicObjectsCount);

        for (int i = 0; i < copyCount; i++)
        {
            var originalValue = dynamicObjects[i]?.Get();
            var copyValue = copy[i]?.Get();
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

        var copy = entity.Clone();

        copy["Id"].ShouldBe(1);
        copy["Name"].ShouldBe("Product E. Name");
        copy["Price"].ShouldBe(1234.56m);
    }

    [Fact]
    public void Should_serialize_dynamic_object_for_empty_array()
    {
        var emptyCollection = new DynamicObjectMapper().MapObject(new ProductEntity[0]);

        emptyCollection.IsNull.ShouldBeFalse();
        emptyCollection.Properties.ShouldHaveSingleItem().Name.ShouldBeEmpty();
        emptyCollection.GetValues().ShouldHaveSingleItem().ShouldBeOfType<object[]>().ShouldBeEmpty();

        var copy = emptyCollection.Clone();

        copy.IsNull.ShouldBeFalse();
        copy.Properties.ShouldHaveSingleItem().Name.ShouldBeEmpty();
        copy.GetValues().ShouldHaveSingleItem().ShouldBeOfType<object[]>().ShouldBeEmpty();
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

        var copy = entity.Clone();

        var item0 = copy[0];
        item0["Id"].ShouldBe(1);
        item0["Name"].ShouldBe("Product E. Name");
        item0["Price"].ShouldBe(1234.56m);

        var item1 = copy[1];
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

        var copy = entity.Clone();

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

        var copy = entity.Clone();

        copy["Id"].ShouldBe(1);
        var items = copy["Items"].ShouldBeAssignableTo<ICollection<DynamicObject>>();
        items.Count.ShouldBe(2);
    }

    [Fact(Skip = "Circular references are not supported")]
    public void Should_serialize_dynamic_object_with_cirtcular_reference()
    {
        var a = new A();
        var b = new A { Reference = a };
        var c = new A { Reference = b };
        a.Reference = c;

        var entity = new DynamicObjectMapper().MapObject(a);

        var copy = entity.Clone();

        copy.ShouldBeOfType<DynamicObject>()[
            nameof(a.Reference)].ShouldBeOfType<DynamicObject>()[
            nameof(b.Reference)].ShouldBeOfType<DynamicObject>()[
            nameof(c.Reference)].ShouldBeSameAs(copy);
    }
}