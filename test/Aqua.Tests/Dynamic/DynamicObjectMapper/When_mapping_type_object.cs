// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper
{
    using Aqua.Dynamic;
    using Aqua.TypeSystem;
    using Shouldly;
    using System;
    using Xunit;

    public class When_mapping_type_object
    {
        DynamicObject dynamicObject;
        Type resurectedType;
        TypeInfo resurectedTypeInfo;

        public When_mapping_type_object()
        {
            dynamicObject = new DynamicObjectMapper().MapObject(typeof(int));
            resurectedTypeInfo = (TypeInfo)new DynamicObjectMapper().Map(dynamicObject);
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

        [Fact]
        public void TypeInfo_type_should_be_typeof_int()
        {
            resurectedTypeInfo.Type.ShouldBe(typeof(int));
        }

        [Fact]
        public void TypeInfo_name_should_be_nameof_int()
        {
            resurectedTypeInfo.Name.ShouldBe(typeof(int).Name);
        }

        [Fact]
        public void TypeInfo_namespace_should_be_system()
        {
            resurectedTypeInfo.Namespace.ShouldBe(typeof(int).Namespace);
        }

        [Fact]
        public void Type_should_be_typeof_int()
        {
            resurectedType.ShouldBe(typeof(int));
        }
    }
}
