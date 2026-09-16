// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Tests.Mappers.TypeInfoMapper;

using Aqua.Protobuf.Mappers;
using Aqua.TypeSystem;

public sealed class When_mapping_type_info
{
    [Fact]
    public void Should_round_trip_null() => MapperTestHelper.RoundTrip(TypeInfoMapper.Instance, default(TypeInfo)).ShouldBeNull();

    [Fact]
    public void Should_round_trip_complete_type_graph()
    {
        var declaringType = new TypeInfo { Name = "Outer", Namespace = "Example" };
        var propertyType = new TypeInfo { Name = "String", Namespace = "System" };
        var value = new TypeInfo
        {
            Name = "Inner",
            Namespace = "Example",
            DeclaringType = declaringType,
            GenericArguments = [new TypeInfo { Name = "Int32", Namespace = "System" }],
            IsAnonymousType = true,
            IsGenericType = true,
            Properties = [new PropertyInfo("Name", propertyType, declaringType) { IsStatic = false }],
        };

        var result = MapperTestHelper.RoundTrip(TypeInfoMapper.Instance, value);

        result.Name.ShouldBe("Inner");
        result.Namespace.ShouldBe("Example");
        result.DeclaringType.Name.ShouldBe("Outer");
        result.GenericArguments.Single().Name.ShouldBe("Int32");
        result.IsAnonymousType.ShouldBeTrue();
        result.IsGenericType.ShouldBeTrue();
        result.Properties.Single().Name.ShouldBe("Name");
        result.Properties.Single().PropertyType.Name.ShouldBe("String");
    }

    [Fact]
    public void Should_restore_shared_reference_identity_when_preserving_references()
    {
        var shared = new TypeInfo { Name = "Shared" };
        var value = new TypeInfo { Name = "Root", DeclaringType = shared, GenericArguments = [shared] };

        var result = MapperTestHelper.RoundTrip(TypeInfoMapper.Instance, value, new ProtoOptions { ReferenceHandler = ReferenceHandler.Preserve });

        result.DeclaringType.ShouldBeSameAs(result.GenericArguments.Single());
    }
}
