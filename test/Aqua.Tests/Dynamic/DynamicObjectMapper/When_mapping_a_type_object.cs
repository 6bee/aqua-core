// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper
{
    using System;
    using Aqua.Dynamic;
    using Xunit;
    using Shouldly;
    using Aqua.TypeSystem;

    public class When_mapping_a_type_object
    {
        DynamicObject dynamicObject;
        Type resurectedType;
        TypeInfo resurectedTypeInfo;

        public When_mapping_a_type_object()
        {
            dynamicObject = new DynamicObjectMapper().MapObject(typeof(int));
            resurectedTypeInfo = new DynamicObjectMapper().Map<TypeInfo>(dynamicObject);
            resurectedType = new DynamicObjectMapper().Map<Type>(dynamicObject);
        }

        [Fact]
        public void Dynamic_object_type_should_typeinfo()
        {
            dynamicObject.Type.Type.ShouldBe(typeof(TypeInfo));
        }

        [Fact]
        public void Dynamic_object_should_hold_type_name()
        {
            dynamicObject[nameof(TypeInfo.Name)].ShouldBe(typeof(int).Name);
        }

        [Fact]
        public void Dynamic_object_should_hold_type_namespace()
        {
            dynamicObject[nameof(TypeInfo.Namespace)].ShouldBe(typeof(int).Namespace);
        }
    }
}
