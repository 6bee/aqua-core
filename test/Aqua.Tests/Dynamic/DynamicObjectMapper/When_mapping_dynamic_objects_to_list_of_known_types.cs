// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper
{
    using Aqua.Dynamic;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class When_mapping_dynamic_objects_to_list_of_known_types
    {
        private class IsKnownTypeProvider : IIsKnownTypeProvider
        {
            public bool IsKnownType(Type type) => type == typeof(CustomReferenceType);
        }

        private class CustomReferenceType
        {
            public int Int32Property { get; set; }

            public string StringProperty { get; set; }
        }

        private readonly DynamicObject[] dynamicObjects;
        private readonly IEnumerable<CustomReferenceType> recreatedObjectLists;

        public When_mapping_dynamic_objects_to_list_of_known_types()
        {
            dynamicObjects = new[]
            {
                new DynamicObject(typeof(CustomReferenceType))
                {
                    Properties = new PropertySet
                    {
                        { string.Empty, new CustomReferenceType { Int32Property = 1, StringProperty = "One" } },
                    },
                },
                new DynamicObject(typeof(CustomReferenceType))
                {
                    Properties = new PropertySet
                    {
                        { string.Empty, new CustomReferenceType { Int32Property = 2, StringProperty = "Two" } },
                    },
                },
            };

            var mapper = new DynamicObjectMapper(isKnownTypeProvider: new IsKnownTypeProvider());
            recreatedObjectLists = dynamicObjects.Select(mapper.Map<CustomReferenceType>);
        }

        [Fact]
        public void Objects_count_should_be_two()
        {
            recreatedObjectLists.Count().ShouldBe(2);
        }

        [Fact]
        public void Objects_should_be_source_objects()
        {
            for (int i = 0; i < dynamicObjects.Length; i++)
            {
                var sourceObject = dynamicObjects.ElementAt(i).Values.Single();

                var recreatedObject = recreatedObjectLists.ElementAt(i);

                recreatedObject.ShouldBeSameAs(sourceObject);
            }
        }
    }
}
