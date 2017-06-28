// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper
{
    using Aqua.Dynamic;
    using Aqua.TypeSystem;
    using Shouldly;
    using System;
    using Xunit;

    public class When_mapping_types
    {
        [Theory]
        [MemberData(nameof(TestData.Types), MemberType = typeof(TestData))]
        public void Should_map_type_to_dynamic_object_and_back(Type type)
        {
            var dynamicObject = new DynamicObjectMapper().MapObject(type);
            var resurectedTypeInfo = (TypeInfo)new DynamicObjectMapper().Map(dynamicObject);
            var resurectedType = new DynamicObjectMapper().Map<Type>(dynamicObject);

            dynamicObject.Type.Type.ShouldBe(typeof(TypeInfo));
            dynamicObject[nameof(TypeInfo.Name)].ShouldBe(type.Name);
            dynamicObject[nameof(TypeInfo.Namespace)].ShouldBe(type.Namespace);

            resurectedTypeInfo.Type.ShouldBe(type);
            resurectedTypeInfo.Name.ShouldBe(type.Name);
            resurectedTypeInfo.Namespace.ShouldBe(type.Namespace);

            resurectedType.ShouldBe(type);
        }
    }
}
