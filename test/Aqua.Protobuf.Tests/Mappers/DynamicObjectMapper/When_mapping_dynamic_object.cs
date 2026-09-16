// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Tests.Mappers.DynamicObjectMapper;

using Aqua.Dynamic;
using Aqua.Protobuf.Mappers;
using Aqua.TypeSystem;

public sealed class When_mapping_dynamic_object
{
    [Fact]
    public void Should_round_trip_null() => MapperTestHelper.RoundTrip(Aqua.Protobuf.Mappers.DynamicObjectMapper.Instance, default(DynamicObject)).ShouldBeNull();

    [Fact]
    public void Should_preserve_null_properties()
    {
        var result = MapperTestHelper.RoundTrip(Aqua.Protobuf.Mappers.DynamicObjectMapper.Instance, new DynamicObject((PropertySet)null));

        result.IsNull.ShouldBeTrue();
        result.Properties.ShouldBeNull();
    }

    [Fact]
    public void Should_round_trip_typed_properties()
    {
        var value = new DynamicObject(typeof(string), new PropertySet([("Length", (object)4)]));

        var result = MapperTestHelper.RoundTrip(Aqua.Protobuf.Mappers.DynamicObjectMapper.Instance, value);

        result.Type.Name.ShouldBe("String");
        result.Get<int>("Length").ShouldBe(4);
    }

    [Fact]
    public void Should_restore_shared_type_identity_when_preserving_references()
    {
        var type = new TypeInfo { Name = "Shared" };
        var value = new DynamicObject(type, new PropertySet([("Type", (object)type)]));

        var result = MapperTestHelper.RoundTrip(Aqua.Protobuf.Mappers.DynamicObjectMapper.Instance, value, new ProtoOptions { ReferenceHandler = ReferenceHandler.Preserve });

        result.Type.ShouldBeSameAs(result.Get<TypeInfo>("Type"));
    }
}
