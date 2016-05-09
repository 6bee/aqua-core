// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper.CustomMapper
{
    using Aqua.Dynamic;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Xunit;
    using Shouldly;
    using System.Reflection;

    public class When_mapping_reducing_property_set
    {
        class DataObject
        {
            public string PropertyOne { get; set; }

            public string PropertyTwo { get; set; }
        }

        class CustomMapper : DynamicObjectMapper
        {
            protected override IEnumerable<PropertyInfo> GetPropertiesForMapping(Type type)
            {
                if (type == typeof(DataObject))
                {
                    return new[] { type.GetProperty("PropertyTwo") };
                }

                return null;
            }
        }

        DynamicObject dynamicObject;

        public When_mapping_reducing_property_set()
        {
            var dynamicObjectMapper = new CustomMapper();

            dynamicObject = dynamicObjectMapper.MapObject(new DataObject
            {
                PropertyOne = "one",
                PropertyTwo = "two"
            });
        }

        [Fact]
        public void Dynamic_object_should_contain_property_two_only()
        {
            dynamicObject.MemberNames.Single().ShouldBe("PropertyTwo");

            dynamicObject["PropertyTwo"].ShouldBe("two");
        }
    }
}
