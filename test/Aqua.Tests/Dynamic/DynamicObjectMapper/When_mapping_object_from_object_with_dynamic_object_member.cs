// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper;

using Aqua.Dynamic;
using Shouldly;
using Xunit;

public class When_mapping_object_from_object_with_dynamic_object_member
{
    private class CustomClass
    {
        public object Reference { get; set; }
    }

    private readonly CustomClass source;
    private readonly DynamicObject dynamicObject;

    public When_mapping_object_from_object_with_dynamic_object_member()
    {
        source = new CustomClass { Reference = new DynamicObject() };
        dynamicObject = new DynamicObjectMapper().MapObject(source);
    }

    [Fact]
    public void Dynamic_object_reference_should_be_same()
    {
        dynamicObject["Reference"].ShouldBeSameAs(source.Reference);
    }
}
