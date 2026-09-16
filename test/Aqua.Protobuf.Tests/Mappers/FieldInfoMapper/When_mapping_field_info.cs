// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Tests.Mappers.FieldInfoMapper;

using Aqua.Protobuf.Mappers;
using Aqua.TypeSystem;

public sealed class When_mapping_field_info
{
    [Fact]
    public void Should_round_trip_null() => MapperTestHelper.RoundTrip(FieldInfoMapper.Instance, default(FieldInfo)).ShouldBeNull();

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Should_round_trip_field_details(bool isStatic)
    {
        var result = MapperTestHelper.RoundTrip(FieldInfoMapper.Instance, new FieldInfo("Value", new TypeInfo { Name = "Container" }) { IsStatic = isStatic });

        result.Name.ShouldBe("Value");
        result.IsStatic.ShouldBe(isStatic);
        result.DeclaringType.Name.ShouldBe("Container");
    }

    [Fact]
    public void Should_preserve_null_static_flag()
    {
        var result = MapperTestHelper.RoundTrip(FieldInfoMapper.Instance, new FieldInfo("Value", new TypeInfo { Name = "Container" }));

        result.IsStatic.ShouldBeNull();
    }
}
