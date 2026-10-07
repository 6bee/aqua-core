// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Tests.AquaProtobufSerializer;

using Aqua.Protobuf;
using System.Buffers;
using System.IO;

public abstract class When_serializing_and_deserializing_a_value
{
    [Fact]
    public void Should_round_trip_a_typed_value()
    {
        var actual = RoundTrip(42);

        actual.ShouldBe(42);
    }

    [Fact]
    public void Should_round_trip_a_string_value()
    {
        var actual = RoundTrip("aqua");

        actual.ShouldBe("aqua");
    }

    [Fact]
    public void Should_round_trip_an_untyped_value()
    {
        var actual = RoundTripUntyped("aqua");

        actual.ShouldBe("aqua");
    }

    /// <summary>
    /// Serializes <paramref name="value"/> using this transport and deserializes it back as
    /// <typeparamref name="T"/>.
    /// </summary>
    protected abstract T RoundTrip<T>(T value);

    /// <summary>
    /// Serializes <paramref name="value"/> using this transport and deserializes it back without a target type.
    /// </summary>
    protected abstract object RoundTripUntyped(object value);

    public class Via_byte_array : When_serializing_and_deserializing_a_value
    {
        protected override T RoundTrip<T>(T value)
        {
            var data = AquaProtobufSerializer.Serialize(value);

            return AquaProtobufSerializer.Deserialize<T>(data);
        }

        protected override object RoundTripUntyped(object value)
        {
            var data = AquaProtobufSerializer.Serialize(value);

            return AquaProtobufSerializer.Deserialize(data);
        }
    }

    public class Via_memory_stream : When_serializing_and_deserializing_a_value
    {
        protected override T RoundTrip<T>(T value)
        {
            using var stream = new MemoryStream();
            AquaProtobufSerializer.Serialize(value, stream);
            stream.Position = 0;

            return AquaProtobufSerializer.Deserialize<T>(stream);
        }

        protected override object RoundTripUntyped(object value)
        {
            using var stream = new MemoryStream();
            AquaProtobufSerializer.Serialize(value, stream);
            stream.Position = 0;

            return AquaProtobufSerializer.Deserialize(stream);
        }
    }
    public class Via_memory_span : When_serializing_and_deserializing_a_value
    {
        protected override T RoundTrip<T>(T value)
        {
            // Measure the encoded size with the byte[] overload so the span is sized exactly, then
            // serialize into the span and read it back from a ReadOnlySpan.
            var encoded = AquaProtobufSerializer.Serialize(value);
            var buffer = new byte[encoded.Length];

            AquaProtobufSerializer.Serialize(value, buffer);

            return AquaProtobufSerializer.Deserialize<T>(buffer.AsSpan());
        }

        protected override object RoundTripUntyped(object value)
        {
            var encoded = AquaProtobufSerializer.Serialize(value);
            var buffer = new byte[encoded.Length];

            AquaProtobufSerializer.Serialize(value, buffer);

            return AquaProtobufSerializer.Deserialize(buffer.AsSpan());
        }
    }
    public class Via_buffer_writer : When_serializing_and_deserializing_a_value
    {
        protected override T RoundTrip<T>(T value)
        {
            var writer = new ArrayBufferWriter<byte>();
            AquaProtobufSerializer.Serialize(value, writer);
            var data = new ReadOnlySequence<byte>(writer.WrittenMemory);

            return AquaProtobufSerializer.Deserialize<T>(data);
        }

        protected override object RoundTripUntyped(object value)
        {
            var writer = new ArrayBufferWriter<byte>();
            AquaProtobufSerializer.Serialize(value, writer);
            var data = new ReadOnlySequence<byte>(writer.WrittenMemory);

            return AquaProtobufSerializer.Deserialize(data);
        }
    }
}
