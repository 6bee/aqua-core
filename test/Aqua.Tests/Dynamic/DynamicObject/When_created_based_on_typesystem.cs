// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using System;
    using System.Reflection;
    using Xunit;

    public class When_created_based_on_typesystem
    {
        public int Property { get; set; }

        private int _field = 0;

        [Fact]
        public void Should_map_method_info()
        {
            var m = GetType().GetMethod(nameof(Should_map_method_info));
            var o = new DynamicObject(m);
            var r = new DynamicObjectMapper().Map<MethodInfo>(o);
        }

        [Fact]
        public void Should_map_property_info()
        {
            var property = GetType().GetProperty(nameof(Property));
            var o = new DynamicObject(property);
            var r = new DynamicObjectMapper().Map<PropertyInfo>(o);
        }

        [Fact]
        public void Should_map_field_info()
        {
            var field = GetType().GetField(nameof(_field), BindingFlags.NonPublic | BindingFlags.Instance);
            var o = new DynamicObject(field);
            var r = new DynamicObjectMapper().Map<FieldInfo>(o);
        }

        [Fact]
        public void Should_map_constructor_info()
        {
            var ctor = GetType().GetConstructor(new Type[0]);
            var o = new DynamicObject(ctor);
            var r = new DynamicObjectMapper().Map<ConstructorInfo>(o);
        }

        [Fact]
        public void Should_map_type_info()
        {
            var type = GetType().GetTypeInfo();
            var o = new DynamicObjectMapper().MapObject(type);
            var r1 = new DynamicObjectMapper().Map<TypeInfo>(o);
            var r2 = new DynamicObjectMapper().Map<Type>(o);
        }
    }
}
