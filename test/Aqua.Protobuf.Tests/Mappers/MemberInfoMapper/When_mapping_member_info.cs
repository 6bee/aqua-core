// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Tests.Mappers.MemberInfoMapper;

using Aqua.Protobuf.Mappers;
using Aqua.TypeSystem;

public sealed class When_mapping_member_info
{
    public static IEnumerable<object[]> Members()
    {
        var type = new TypeInfo { Name = "Container" };
        yield return [new FieldInfo("Field", type)];
        yield return [new PropertyInfo("Property", new TypeInfo { Name = "String" }, type)];
        yield return [new MethodInfo("Method", type)];
        yield return [new ConstructorInfo { Name = ".ctor", DeclaringType = type }];
    }

    [Fact]
    public void Should_round_trip_null() => MapperTestHelper.RoundTrip(MemberInfoMapper.Instance, default(MemberInfo)).ShouldBeNull();

    [Theory]
    [MemberData(nameof(Members))]
    public void Should_round_trip_supported_member_kind(MemberInfo value)
    {
        var result = MapperTestHelper.RoundTrip(MemberInfoMapper.Instance, value);

        result.GetType().ShouldBe(value.GetType());
        result.Name.ShouldBe(value.Name);
    }
}
