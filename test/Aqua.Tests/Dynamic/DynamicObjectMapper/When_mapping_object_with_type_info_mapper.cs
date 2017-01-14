// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper
{
    using System;
    using Aqua.Dynamic;
    using Xunit;
    using Shouldly;

    public class When_mapping_object_with_type_info_mapper
    {
        class TypeMapper : ITypeMapper
        {
            public Type MapType(Type type) => typeof(B);
        }

        class A { }

        class B { }

        DynamicObject dynamicObject;

        public When_mapping_object_with_type_info_mapper()
        {
            dynamicObject = new DynamicObjectMapper(typeMapper: new TypeMapper()).MapObject(new A());
        }

        [Fact]
        public void Dynamic_object_type_should_reflect_mapper_result()
        {
            dynamicObject.Type.Type.ShouldBe(typeof(B));
        }
    }
}
