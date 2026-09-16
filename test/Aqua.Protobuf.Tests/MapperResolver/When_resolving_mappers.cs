// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Tests.MapperResolver;

using Aqua.Protobuf;
using Aqua.Protobuf.Mappers;
using Google.Protobuf;
using Proto = Schema;

public class When_adding_a_mapper_to_a_resolver
{
    [Fact]
    public void Should_resolve_the_mapper_by_value_type()
    {
        var resolver = new MapperResolver();

        resolver.TryAddMapper(ValueMapper<int>.Instance).ShouldBeTrue();

        resolver.GetMapper<int>().ShouldBeSameAs(ValueMapper<int>.Instance);
        resolver.GetMapper<string>().ShouldBeNull();
    }
}

public class When_adding_a_duplicate_mapper_to_a_resolver
{
    [Fact]
    public void Should_return_false()
    {
        var resolver = new MapperResolver();
        resolver.TryAddMapper(ValueMapper<int>.Instance).ShouldBeTrue();

        resolver.TryAddMapper(ValueMapper<int>.Instance).ShouldBeFalse();
    }
}

public class When_adding_a_duplicate_mapper_with_the_extension
{
    [Fact]
    public void Should_throw_an_invalid_operation_exception()
    {
        var resolver = new MapperResolver();
        resolver.AddMapper(ValueMapper<int>.Instance);

        Should.Throw<InvalidOperationException>(() => resolver.AddMapper(ValueMapper<int>.Instance)).Message.ShouldContain("already been registered");
    }
}

public class When_adding_aqua_types_to_a_resolver
{
    [Fact]
    public void Should_register_scalar_and_dynamic_mappers()
    {
        var resolver = new MapperResolver().AddAquaTypes();

        resolver.GetMapper<int>().ShouldNotBeNull();
        resolver.GetMapper<string>().ShouldNotBeNull();
        resolver.GetMapper<Dynamic.DynamicObject>().ShouldNotBeNull();
    }
}

public class When_getting_a_registered_mapper_with_verification
{
    [Fact]
    public void Should_return_the_mapper()
    {
        IMapperResolver resolver = new MapperResolver().AddMapper(ValueMapper<int>.Instance);

        resolver.GetMapperWithVerify<int>().ShouldBeSameAs(ValueMapper<int>.Instance);
    }
}

public class When_getting_an_unregistered_mapper_with_verification
{
    [Fact]
    public void Should_throw_a_mapper_not_registered_exception()
    {
        IMapperResolver resolver = new MapperResolver();

        Should.Throw<MapperNotRegisteredException>(() => resolver.GetMapperWithVerify<int>());
    }
}

public class When_optimizing_a_mapper_resolver
{
    [Fact]
    public void Should_return_the_same_optimized_resolver()
    {
        IMapperResolver resolver = new MapperResolver();
        var optimized = resolver.Optimized();

        optimized.Optimized().ShouldBeSameAs(optimized);
    }
}

public class When_resolving_a_mapper_from_an_optimized_resolver
{
    [Fact]
    public void Should_cache_the_first_resolved_mapper()
    {
        var mapper = new MarkerMapper();
        IMapperResolver resolver = new MapperResolver().AddMapper(mapper);

        resolver.Optimized().GetMapper<Marker>().ShouldBeSameAs(mapper);
    }

    private sealed class Marker;

    private sealed class MarkerMapper : IProtoMapper<Marker, Proto.Value>
    {
        public Proto.Value ToProto(Marker value, ProtoContext context) => new();

        public Marker FromProto(Proto.Value proto, ProtoContext context) => new();

        IMessage IProtoMapper<Marker>.ToProto(Marker value, ProtoContext context) => ToProto(value, context);

        Marker IProtoMapper<Marker>.FromProto(IMessage proto, ProtoContext context) => FromProto((Proto.Value)proto, context);
    }
}
