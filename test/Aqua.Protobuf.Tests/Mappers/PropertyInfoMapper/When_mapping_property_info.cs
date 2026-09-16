// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Tests.Mappers.PropertyInfoMapper;

using Aqua.Protobuf.Mappers;
using Aqua.TypeSystem;

public sealed class When_mapping_property_info
{
    [Fact]
    public void Should_round_trip_null() => MapperTestHelper.RoundTrip(PropertyInfoMapper.Instance, default(PropertyInfo)).ShouldBeNull();

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Should_round_trip_property_details(bool isStatic)
    {
        var result = MapperTestHelper.RoundTrip(PropertyInfoMapper.Instance, new PropertyInfo("Name", new TypeInfo { Name = "String" }, new TypeInfo { Name = "Container" }) { IsStatic = isStatic });

        result.Name.ShouldBe("Name");
        result.IsStatic.ShouldBe(isStatic);
        result.DeclaringType.Name.ShouldBe("Container");
        result.PropertyType.Name.ShouldBe("String");
    }

    [Fact]
    public void Should_preserve_null_static_flag()
    {
        var result = MapperTestHelper.RoundTrip(PropertyInfoMapper.Instance, new PropertyInfo("Name", new TypeInfo { Name = "String" }, new TypeInfo { Name = "Container" }));

        result.IsStatic.ShouldBeNull();
    }
}
