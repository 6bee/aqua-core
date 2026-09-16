// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Tests.AquaProtobufSerializer;

using Aqua.Protobuf;
using System.Buffers;
using System.IO;

public class When_serializing_a_value_to_a_byte_array
{
    [Fact]
    public void Should_deserialize_the_typed_and_untyped_value()
    {
        var data = AquaProtobufSerializer.Serialize("aqua");

        AquaProtobufSerializer.Deserialize<string>(data).ShouldBe("aqua");
        AquaProtobufSerializer.Deserialize(data).ShouldBe("aqua");
    }
}

public class When_serializing_a_value_to_a_stream
{
    [Fact]
    public void Should_deserialize_the_typed_and_untyped_value()
    {
        using var stream = new MemoryStream();
        AquaProtobufSerializer.Serialize(42, stream);
        stream.Position = 0;

        AquaProtobufSerializer.Deserialize<int>(stream).ShouldBe(42);
        stream.Position = 0;
        AquaProtobufSerializer.Deserialize(stream).ShouldBe(42);
    }
}

public class When_serializing_a_value_to_a_span
{
    [Fact]
    public void Should_deserialize_the_value()
    {
        var expected = AquaProtobufSerializer.Serialize(42);
        var buffer = new byte[expected.Length];

        AquaProtobufSerializer.Serialize(42, buffer);

        AquaProtobufSerializer.Deserialize<int>(buffer.AsSpan()).ShouldBe(42);
        AquaProtobufSerializer.Deserialize(buffer.AsSpan()).ShouldBe(42);
    }
}

public class When_serializing_a_value_to_a_buffer_writer
{
    [Fact]
    public void Should_deserialize_the_value_from_a_read_only_sequence()
    {
        var writer = new ArrayBufferWriter<byte>();
        AquaProtobufSerializer.Serialize(42, writer);
        var data = new ReadOnlySequence<byte>(writer.WrittenMemory);

        AquaProtobufSerializer.Deserialize<int>(data).ShouldBe(42);
        AquaProtobufSerializer.Deserialize(data).ShouldBe(42);
    }
}

public class When_mapping_a_value_through_a_proto_context
{
    [Fact]
    public void Should_round_trip_the_value_and_support_a_typed_proto()
    {
        var writeContext = ProtoContext.ForWrite();
        var proto = writeContext.ToProto<int, Schema.Value>(42);

        ProtoContext.ForRead().FromProto<int>(proto).ShouldBe(42);
    }
}

public class When_serializing_a_value_through_a_proto_context
{
    [Fact]
    public void Should_support_all_transport_overloads()
    {
        var context = ProtoContext.ForWrite();
        var expected = context.Serialize(42);
        var span = new byte[expected.Length];
        var writer = new ArrayBufferWriter<byte>();
        using var stream = new MemoryStream();

        context.Serialize(42, span);
        context.Serialize(42, writer);
        context.Serialize(42, stream);

        ProtoContext.ForRead().Deserialize<int>(expected).ShouldBe(42);
        ProtoContext.ForRead().Deserialize<int>(span.AsSpan()).ShouldBe(42);
        ProtoContext.ForRead().Deserialize<int>(new ReadOnlySequence<byte>(writer.WrittenMemory)).ShouldBe(42);
        stream.Position = 0;
        ProtoContext.ForRead().Deserialize<int>(stream).ShouldBe(42);
    }
}
