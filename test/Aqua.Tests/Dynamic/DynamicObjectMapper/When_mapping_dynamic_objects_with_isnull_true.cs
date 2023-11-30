// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper;

using Aqua.Dynamic;
using Shouldly;
using Xunit;

public class When_mapping_dynamic_objects_with_isnull_true
{
    [Fact]
    public void Should_map_as_nullable_valuetype()
    {
        var obj = new DynamicObject { IsNull = true };

        var result = new DynamicObjectMapper().Map<int?>(obj);

        result.ShouldBeNull();
    }

    [Fact]
    public void Should_map_as_valuetype()
    {
        var obj = new DynamicObject { IsNull = true };

        var result = new DynamicObjectMapper().Map<int>(obj);

        result.ShouldBe(0);
    }
}
