// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if COREFX

namespace Aqua.Tests.ProtoBuf
{
    using Aqua.Dynamic;
    using Shouldly;
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Linq;
    using System.Numerics;
    using Xunit;
    using static Aqua.Tests.Serialization.ProtobufNetSerializationHelper;

    public class When_serializing_dynamic_types
    {
        [SkippableTheory]
        [MemberData(nameof(TestData.NativeValues), MemberType = typeof(TestData))]
        [MemberData(nameof(TestData.NativeValueArrays), MemberType = typeof(TestData))]
        [MemberData(nameof(TestData.NativeValueLists), MemberType = typeof(TestData))]
        public void Should_serialize_property(Type type, object value, CultureInfo culture)
        {
            SkipUnsupportedDataType(type, value);

            using var cultureContext = culture.CreateContext();

            var proeprty = new Property("p1", value);
            var copy = proeprty.Serialize();

            copy.Value.ShouldBe(value);
        }

        [SkippableFact]
        public void Should_serialize_property_set()
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
        }

        [SkippableTheory]
        [MemberData(nameof(TestData.NativeValues), MemberType = typeof(TestData))]
        [MemberData(nameof(TestData.NativeValueArrays), MemberType = typeof(TestData))]
        [MemberData(nameof(TestData.NativeValueLists), MemberType = typeof(TestData))]
        public void Should_serialize_property_set_2(Type type, object value, CultureInfo culture)
        {
            SkipUnsupportedDataType(type, value);

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
            SkipUnsupportedDataType(type, value);

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
            SkipUnsupportedDataType(type, value);

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
    }
}

#endif // COREFX
