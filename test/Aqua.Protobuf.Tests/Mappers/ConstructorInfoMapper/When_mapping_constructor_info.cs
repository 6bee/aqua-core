// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Tests.Mappers.ConstructorInfoMapper;

using Aqua.Protobuf.Mappers;
using Aqua.TypeSystem;

public sealed class When_mapping_constructor_info
{
    [Fact]
    public void Should_round_trip_null() => MapperTestHelper.RoundTrip(ConstructorInfoMapper.Instance, default(ConstructorInfo)).ShouldBeNull();

    [Fact]
    public void Should_round_trip_constructor_details()
    {
        var result = MapperTestHelper.RoundTrip(ConstructorInfoMapper.Instance, new ConstructorInfo
        {
            Name = ".ctor",
            DeclaringType = new TypeInfo { Name = "Container" },
            GenericArgumentTypes = [new TypeInfo { Name = "T" }],
            ParameterTypes = [new TypeInfo { Name = "String" }],
            IsStatic = false,
        });

        result.Name.ShouldBe(".ctor");
        result.IsStatic.ShouldBe(false);
        result.DeclaringType.Name.ShouldBe("Container");
        result.GenericArgumentTypes.Single().Name.ShouldBe("T");
        result.ParameterTypes.Single().Name.ShouldBe("String");
    }
}
