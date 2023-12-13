// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper;

using Aqua.Dynamic;
using Shouldly;
using Xunit;

public class When_mapping_object_from_dynamic_object
{
    private readonly DynamicObject source;
    private readonly DynamicObject dynamicObject;

    public When_mapping_object_from_dynamic_object()
    {
        source = new DynamicObject();
        dynamicObject = new DynamicObjectMapper().MapObject(source);
    }

    [Fact]
    public void Dynamic_object_should_be_different_instance()
    {
        dynamicObject.ShouldBeSameAs(source);
    }
}