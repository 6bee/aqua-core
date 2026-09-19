// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper;

using Aqua.Dynamic;

public class When_mapping_multidimensional_array_with_empty_dimension
{
    [Theory]
    [InlineData(0, 2, 3)]
    [InlineData(2, 0, 3)]
    [InlineData(2, 3, 0)]
    public void Empty_dimension_should_be_preserved(int length0, int length1, int length2)
    {
        var source = new int[length0, length1, length2];

        var dynamicObject = DynamicObject.Create(source);
        var result = dynamicObject.CreateObject().ShouldBeOfType<int[,,]>();

        result.GetLength(0).ShouldBe(length0);
        result.GetLength(1).ShouldBe(length1);
        result.GetLength(2).ShouldBe(length2);
        dynamicObject.Get("Dimensions").ShouldBeOfType<object[]>().ShouldBe([length0, length1, length2]);
        dynamicObject.Get().ShouldBeOfType<object[]>().ShouldBeEmpty();
    }
}
