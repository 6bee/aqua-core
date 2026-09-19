// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TypeSystem.TypeInfo;

using Aqua.TypeSystem;

public class When_creating_type_info_for_jagged_3d_array_type
{
    private readonly TypeInfo typeInfo = new(typeof(int[][][]));

    [Fact]
    public void Type_info_should_have_is_array_true()
    {
        typeInfo.IsArray.ShouldBeTrue();
    }

    [Fact]
    public void Type_info_name_should_have_array_brackets()
    {
        typeInfo.Name.ShouldBe("Int32[][][]");
    }

    [Fact]
    public void Type_info_fullname_should_have_array_brackets()
    {
        typeInfo.FullName.ShouldBe("System.Int32[][][]");
    }

    [Fact]
    public void Type_info_should_map_to_original_array_type()
    {
        typeInfo.ToType().ShouldBe(typeof(int[][][]));
    }

    [Fact]
    public void Type_info_should_map_to_array_type_with_original_array_rank()
    {
        typeInfo.ToType().GetArrayRank().ShouldBe(1);

        typeInfo.ToType().GetElementType()!.GetArrayRank().ShouldBe(1);

        typeInfo.ToType().GetElementType()!.GetElementType()!.GetArrayRank().ShouldBe(1);
    }
}
