// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper
{
    using Aqua.Dynamic;
    using Shouldly;
    using System;
    using System.Globalization;
    using System.Threading;
    using Xunit;

    /// <summary>
    /// Covers mapping type mismatches for unassignable types without validation, i.e. exeption upon assignment.
    /// </summary>
    public class When_mapping_dynamic_object_to_type_with_different_property_types
    {
        private class CustomType
        {
            public int Int32Value { get; set; }
        }

        private readonly DynamicObject dynamicObject;

        public When_mapping_dynamic_object_to_type_with_different_property_types()
        {
            dynamicObject = new DynamicObject
            {
                Properties = new PropertySet
                {
                    { "Int32Value", 1.23456789 },
                },
            };
        }

        [Fact]
        public void Should_throw_when_preventing_type_validation()
        {
#if !NETCOREAPP1_0
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
#endif
            var mapper = new DynamicObjectMapper(new DynamicObjectMapperSettings { SilentlySkipUnassignableMembers = false });

            var ex = Assert.Throws<ArgumentException>(() => mapper.Map<CustomType>(dynamicObject));

            ex.Message.ShouldBe("Object of type 'System.Double' cannot be converted to type 'System.Int32'.");
        }

        [Fact]
        public void Should_silently_skip_unmatching_value_when_allowing_type_validation()
        {
            var mapper = new DynamicObjectMapper(new DynamicObjectMapperSettings { SilentlySkipUnassignableMembers = true });

            var obj = mapper.Map<CustomType>(dynamicObject);

            obj.ShouldNotBeNull();
        }
    }
}