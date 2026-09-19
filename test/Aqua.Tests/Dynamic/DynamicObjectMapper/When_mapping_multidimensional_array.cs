// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper;

using Aqua.Dynamic;

public class When_mapping_multidimensional_array
{
    private readonly int[,,] matrix;
    private readonly DynamicObject dynamicObject;

    public When_mapping_multidimensional_array()
    {
        matrix = new[,,]
        {
            {
                { 1, 2, 3, 4 },
                { 5, 6, 7, 8 }
            },
            {
                { 10, 20, 30, 40 },
                { 50, 60, 70, 80 }
            },
            {
                { 100, 200, 300, 400 },
                { 500, 600, 700, 800 }
            }
        };
        dynamicObject = DynamicObject.Create(matrix);
    }

    [Fact]
    public void Dynamic_object_should_have_empty_name_property_with_flattened_values()
    {
        dynamicObject.Properties.Single(x => x.Name == string.Empty).Value
            .ShouldBeOfType<object[]>()
            .ShouldBe(matrix.Cast<object>().ToArray());
    }

    [Fact]
    public void Dynamic_object_should_have_dimensions_property()
    {
        dynamicObject.Properties.Single(x => x.Name == "Dimensions").Value
            .ShouldBeOfType<object[]>()
            .ShouldBe([3, 2, 4]);
    }

    [Fact]
    public void Dynamic_object_should_be_single_value_wrapper_with_dimensions_metadata()
    {
        dynamicObject.IsSingleValueWrapper().ShouldBeTrue();
    }

    [Fact]
    public void Dynamic_object_type_should_be_multidimensional_array_type()
    {
        dynamicObject.Type.ToType().ShouldBe(typeof(int[,,]));
    }

    [Fact]
    public void Verify_matrix_dimensions() => VerifyMatrixDimensions(matrix);

    [Fact]
    public void Dynamic_object_should_result_in_multidimensional_array_when_mapped_back()
    {
        var matrix = dynamicObject.CreateObject().ShouldBeOfType<int[,,]>();
        VerifyMatrixDimensions(matrix);
    }

    private void VerifyMatrixDimensions(int[,,] matrix)
    {
        matrix.Rank.ShouldBe(3);

        matrix.GetLowerBound(0).ShouldBe(0);
        matrix.GetUpperBound(0).ShouldBe(2);

        matrix.GetLowerBound(1).ShouldBe(0);
        matrix.GetUpperBound(1).ShouldBe(1);

        matrix.GetLowerBound(2).ShouldBe(0);
        matrix.GetUpperBound(2).ShouldBe(3);
    }

    [Fact]
    public void Verify_matrix_values() => VerifyMatrixValues(matrix);

    [Fact]
    public void Dynamic_object_should_map_back_into_original_multidimensional_array_structure()
    {
        var matrix = dynamicObject.CreateObject().ShouldBeOfType<int[,,]>();
        VerifyMatrixValues(matrix);
    }
    
    private void VerifyMatrixValues(int[,,] matrix)
    {
        matrix[0, 0, 0].ShouldBe(1);
        matrix[0, 0, 1].ShouldBe(2);
        matrix[0, 0, 2].ShouldBe(3);
        matrix[0, 0, 3].ShouldBe(4);

        matrix[0, 1, 0].ShouldBe(5);
        matrix[0, 1, 1].ShouldBe(6);
        matrix[0, 1, 2].ShouldBe(7);
        matrix[0, 1, 3].ShouldBe(8);

        matrix[1, 0, 0].ShouldBe(10);
        matrix[1, 0, 1].ShouldBe(20);
        matrix[1, 0, 2].ShouldBe(30);
        matrix[1, 0, 3].ShouldBe(40);

        matrix[1, 1, 0].ShouldBe(50);
        matrix[1, 1, 1].ShouldBe(60);
        matrix[1, 1, 2].ShouldBe(70);
        matrix[1, 1, 3].ShouldBe(80);

        matrix[2, 0, 0].ShouldBe(100);
        matrix[2, 0, 1].ShouldBe(200);
        matrix[2, 0, 2].ShouldBe(300);
        matrix[2, 0, 3].ShouldBe(400);

        matrix[2, 1, 0].ShouldBe(500);
        matrix[2, 1, 1].ShouldBe(600);
        matrix[2, 1, 2].ShouldBe(700);
        matrix[2, 1, 3].ShouldBe(800);
    }
}
