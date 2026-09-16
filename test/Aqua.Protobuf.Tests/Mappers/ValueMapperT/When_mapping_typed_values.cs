// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Tests.Mappers.ValueMapperT;

using Aqua.Protobuf.Mappers;

public sealed class When_mapping_typed_values
{
    [Fact]
    public void Should_round_trip_value_through_generic_mapper()
    {
        var result = MapperTestHelper.RoundTrip(ValueMapper<int>.Instance, 42);

        result.ShouldBe(42);
    }

    [Fact]
    public void Should_round_trip_null_through_nullable_generic_mapper()
    {
        var result = MapperTestHelper.RoundTrip(ValueMapper<int?>.Instance, default(int?));

        result.ShouldBeNull();
    }
}
