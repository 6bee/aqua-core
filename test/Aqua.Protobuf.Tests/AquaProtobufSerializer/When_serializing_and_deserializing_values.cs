// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Tests.AquaProtobufSerializer;

using Aqua.Protobuf;
using System.Buffers;
using System.IO;

public class When_mapping_a_value_through_a_proto_context
{
    [Fact]
    public void Should_round_trip_the_value_and_support_a_typed_proto()
    {
        var proto = new ProtoContext().ToProto<int, Schema.Value>(42);

        new ProtoContext().FromProto<int>(proto).ShouldBe(42);
    }
}

public class When_serializing_a_value_through_fresh_proto_context
{
    [Fact]
    public void Should_support_all_transport_overloads()
    {
        var expected = new ProtoContext().Serialize(42);
        var span = new byte[expected.Length];
        var writer = new ArrayBufferWriter<byte>();
        using var stream = new MemoryStream();

        new ProtoContext().Serialize(42, span);
        new ProtoContext().Serialize(42, writer);
        new ProtoContext().Serialize(42, stream);

        new ProtoContext().Deserialize<int>(expected).ShouldBe(42);
        new ProtoContext().Deserialize<int>(span.AsSpan()).ShouldBe(42);
        new ProtoContext().Deserialize<int>(new ReadOnlySequence<byte>(writer.WrittenMemory)).ShouldBe(42);
        stream.Position = 0;
        new ProtoContext().Deserialize<int>(stream).ShouldBe(42);
    }
}

public class When_round_tripping_with_shared_context_instances
{
    [Fact]
    public void Should_serialize_and_deserialize_using_shared_contexts()
    {
        var context = new ProtoContext();

        var expected = context.Serialize(42);

        context.Deserialize<int>(expected).ShouldBe(42);
    }

    [Fact]
    public void Should_support_all_transport_overloads()
    {
        var context = new ProtoContext();

        var expected = context.Serialize(42);
        var span = new byte[expected.Length];
        var writer = new ArrayBufferWriter<byte>();
        using var stream = new MemoryStream();

        context.Serialize(42, span);
        context.Serialize(42, writer);
        context.Serialize(42, stream);

        context.Deserialize<int>(expected).ShouldBe(42);
        context.Deserialize<int>(span.AsSpan()).ShouldBe(42);
        context.Deserialize<int>(new ReadOnlySequence<byte>(writer.WrittenMemory)).ShouldBe(42);
        stream.Position = 0;
        context.Deserialize<int>(stream).ShouldBe(42);
    }
}
