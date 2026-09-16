// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Tests.Mappers.MethodInfoMapper;

using Aqua.Protobuf.Mappers;
using Aqua.TypeSystem;

public sealed class When_mapping_method_info
{
    [Fact]
    public void Should_round_trip_null() => MapperTestHelper.RoundTrip(MethodInfoMapper.Instance, default(MethodInfo)).ShouldBeNull();

    [Fact]
    public void Should_round_trip_method_details()
    {
        var result = MapperTestHelper.RoundTrip(MethodInfoMapper.Instance, new MethodInfo("Convert", new TypeInfo { Name = "Converter" }, [new TypeInfo { Name = "T" }], [new TypeInfo { Name = "String" }], new TypeInfo { Name = "Boolean" }) { IsStatic = true });

        result.Name.ShouldBe("Convert");
        result.IsStatic.ShouldBe(true);
        result.DeclaringType.Name.ShouldBe("Converter");
        result.GenericArgumentTypes.Single().Name.ShouldBe("T");
        result.ParameterTypes.Single().Name.ShouldBe("String");
        result.ReturnType.Name.ShouldBe("Boolean");
    }
}
