// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper;

using Aqua.Dynamic;
using Aqua.TypeSystem;
using Shouldly;
using Xunit;

public class When_mapping_type_object
{
    private readonly DynamicObject dynamicObject;
    private readonly Type resurectedType;

    public When_mapping_type_object()
    {
        dynamicObject = new DynamicObjectMapper().MapObject(typeof(int?[]));
        resurectedType = (Type)new DynamicObjectMapper().Map(dynamicObject);
    }

    [Fact]
    public void Dynamic_object_type_should_typeinfo()
    {
        dynamicObject.Type.ToType().ShouldBe(typeof(Type));
    }

    [Fact]
    public void Dynamic_object_should_hold_type_name()
    {
        dynamicObject[nameof(TypeInfo.Name)].ShouldBe(typeof(int?[]).Name);
    }

    [Fact]
    public void Dynamic_object_should_hold_type_namespace()
    {
        dynamicObject[nameof(TypeInfo.Namespace)].ShouldBe(typeof(int?[]).Namespace);
    }

    [Fact]
    public void Type_should_be_typeof_int()
    {
        resurectedType.ShouldBe(typeof(int?[]));
    }
}