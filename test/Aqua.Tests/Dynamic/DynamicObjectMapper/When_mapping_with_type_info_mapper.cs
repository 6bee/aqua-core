// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper
{
    using Aqua.Dynamic;
    using Xunit;
    using Xunit.Fluent;

    public class When_mapping_with_type_info_mapper
    {
        class A { }

        class B { }

        DynamicObject dynamicObject;

        public When_mapping_with_type_info_mapper()
        {
            dynamicObject = new DynamicObjectMapper(dynamicObjectTypeInfoMapper: t => typeof(B)).MapObject(new A());
        }

        [Fact]
        public void Dynamic_object_type_should_reflect_mapper_result()
        {
            dynamicObject.Type.Type.ShouldBe(typeof(B));
        }
    }
}
