// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Tests.Options;

using Aqua.Protobuf;

public class When_accessing_the_default_proto_options_resolver
{
    [Fact]
    public void Should_create_and_cache_a_resolver_with_aqua_mappers()
    {
        var options = new ProtoOptions();

        options.Resolver.ShouldBeSameAs(options.Resolver);
        options.Resolver.GetMapper<int>().ShouldNotBeNull();
    }
}

public class When_configuring_proto_options
{
    [Fact]
    public void Should_retain_the_configured_encodings_reference_handler_and_resolver()
    {
        var resolver = new MapperResolver();
        var options = new ProtoOptions
        {
            DateTimeEncoding = DateTimeEncoding.UnixNanoseconds,
            TimeSpanEncoding = TimeSpanEncoding.Nanoseconds,
            ReferenceHandler = ReferenceHandler.Preserve,
            Resolver = resolver,
        };

        options.DateTimeEncoding.ShouldBe(DateTimeEncoding.UnixNanoseconds);
        options.TimeSpanEncoding.ShouldBe(TimeSpanEncoding.Nanoseconds);
        options.ReferenceHandler.ShouldBe(ReferenceHandler.Preserve);
        options.Resolver.ShouldBeSameAs(resolver);
    }
}
