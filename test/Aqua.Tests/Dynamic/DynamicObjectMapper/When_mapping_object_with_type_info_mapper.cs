// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper;

using Aqua.Dynamic;
using Shouldly;
using Xunit;

public class When_mapping_object_with_type_info_mapper
{
    private class TypeMapper : ITypeMapper
    {
        public Type MapType(Type type) => typeof(B);
    }

    private class A;

    private class B;

    private readonly DynamicObject dynamicObject;

    public When_mapping_object_with_type_info_mapper()
    {
        dynamicObject = new DynamicObjectMapper(typeMapper: new TypeMapper()).MapObject(new A());
    }

    [Fact]
    public void Dynamic_object_type_should_reflect_mapper_result()
    {
        dynamicObject.Type.ToType().ShouldBe(typeof(B));
    }
}