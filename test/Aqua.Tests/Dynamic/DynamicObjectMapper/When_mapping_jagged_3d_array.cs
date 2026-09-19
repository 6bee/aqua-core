// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper;

using Aqua.Dynamic;

public class When_mapping_jagged_3d_array
{
    private readonly int[][][] array;
    private readonly DynamicObject dynamicObject;

    public When_mapping_jagged_3d_array()
    {
        array = new int[3][][];

        array[0] = new int[1][];
        array[1] = new int[1][];
        array[2] = new int[1][];

        array[0][0] = [1, 2, 3, 4];
        array[1][0] = [10, 20];
        array[2][0] = [100, 200, 300];

        dynamicObject = DynamicObject.Create(array);
    }

    [Fact]
    public void Dynamic_object_should_have_one_property_with_empty_name_to_represent_jagged_array()
    {
        dynamicObject.Properties.Single().Name.ShouldBe(string.Empty);
    }

    [Fact]
    public void Dynamic_object_type_should_be_jagged_array_type()
    {
        dynamicObject.Type.ToType().ShouldBe(typeof(int[][][]));
    }

    [Fact]
    public void Verify_jagged_dimensions() => VerifyArrayDimensions(array);

    [Fact]
    public void Dynamic_object_should_result_in_jagged_array_when_mapped_back()
    {
        var array = dynamicObject.CreateObject().ShouldBeOfType<int[][][]>();
        VerifyArrayDimensions(array);
    }

    private void VerifyArrayDimensions(int[][][] array)
    {
        array.Rank.ShouldBe(1);
        array.GetLowerBound(0).ShouldBe(0);
        array.GetUpperBound(0).ShouldBe(2);

        array[0].Rank.ShouldBe(1);
        array[0].GetLowerBound(0).ShouldBe(0);
        array[0].GetUpperBound(0).ShouldBe(0);

        array[0][0].Rank.ShouldBe(1);
        array[0][0].GetLowerBound(0).ShouldBe(0);
        array[0][0].GetUpperBound(0).ShouldBe(3);

        array[1][0].Rank.ShouldBe(1);
        array[1][0].GetLowerBound(0).ShouldBe(0);
        array[1][0].GetUpperBound(0).ShouldBe(1);

        array[2][0].Rank.ShouldBe(1);
        array[2][0].GetLowerBound(0).ShouldBe(0);
        array[2][0].GetUpperBound(0).ShouldBe(2);
    }

    [Fact]
    public void Verify_array_values() => VerifyArrayValues(array);

    [Fact]
    public void Dynamic_object_should_map_back_into_original_jagged_array_structure()
    {
        var array = dynamicObject.CreateObject().ShouldBeOfType<int[][][]>();
        VerifyArrayValues(array);
    }

    private void VerifyArrayValues(int[][][] array)
    {
        array[0][0][0].ShouldBe(1);
        array[0][0][1].ShouldBe(2);
        array[0][0][2].ShouldBe(3);
        array[0][0][3].ShouldBe(4);

        array[1][0][0].ShouldBe(10);
        array[1][0][1].ShouldBe(20);

        array[2][0][0].ShouldBe(100);
        array[2][0][1].ShouldBe(200);
        array[2][0][2].ShouldBe(300);
    }
}
