// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf;

using Aqua.Protobuf.Mappers;
using Google.Protobuf;
using System.Buffers;
using System.IO;
using Proto = Aqua.Protobuf.Schema;

/// <summary>
/// Schema-first Google.Protobuf serializer for <i>Aqua</i> types. Object graphs are mapped through a
/// recursive, self-describing protobuf value union.
/// </summary>
/// <remarks>
/// For more granular control either use <see cref="ProtoContext"/> or mapper implementations for specific proto types.
/// </remarks>
public static class AquaProtobufSerializer
{
    /// <summary>
    /// Serializes the specified <paramref name="graph"/> to a protobuf-encoded byte array.
    /// </summary>
    /// <typeparam name="T">The type of the graph to serialize.</typeparam>
    /// <param name="graph">The object graph to serialize.</param>
    /// <param name="options">Protobuf serializer options.</param>
    /// <returns>The protobuf-encoded representation of <paramref name="graph"/>.</returns>
    public static byte[] Serialize<T>(T graph, ProtoOptions? options = null)
    {
        return ToProto(graph, options).ToByteArray();
    }

    /// <summary>
    /// Serializes the specified <paramref name="graph"/> to the given <paramref name="stream"/>.
    /// </summary>
    /// <typeparam name="T">The type of the graph to serialize.</typeparam>
    /// <param name="graph">The object graph to serialize.</param>
    /// <param name="stream">The destination stream.</param>
    /// <param name="options">Protobuf serializer options.</param>
    public static void Serialize<T>(T graph, Stream stream, ProtoOptions? options = null)
    {
        stream.AssertNotNull();
        ToProto(graph, options).WriteTo(stream);
    }

    /// <summary>
    /// Serializes the specified <paramref name="graph"/> to the given <paramref name="span"/>.
    /// </summary>
    /// <typeparam name="T">The type of the graph to serialize.</typeparam>
    /// <param name="graph">The object graph to serialize.</param>
    /// <param name="span">The destination span.</param>
    /// <param name="options">Protobuf serializer options.</param>
    public static void Serialize<T>(T graph, Span<byte> span, ProtoOptions? options = null)
    {
        ToProto(graph, options).WriteTo(span);
    }

    /// <summary>
    /// Serializes the specified <paramref name="graph"/> to the given <paramref name="writer"/>.
    /// </summary>
    /// <typeparam name="T">The type of the graph to serialize.</typeparam>
    /// <param name="graph">The object graph to serialize.</param>
    /// <param name="writer">The destination writer.</param>
    /// <param name="options">Protobuf serializer options.</param>
    public static void Serialize<T>(T graph, IBufferWriter<byte> writer, ProtoOptions? options = null)
    {
        writer.AssertNotNull();
        ToProto(graph, options).WriteTo(writer);
    }

    /// <summary>
    /// Deserializes a graph of type <typeparamref name="T"/> from the protobuf-encoded <paramref name="data"/>.
    /// </summary>
    /// <typeparam name="T">The expected type of the deserialized graph.</typeparam>
    /// <param name="data">The protobuf-encoded representation to deserialize.</param>
    /// <param name="options">Protobuf serializer options.</param>
    /// <returns>The deserialized graph.</returns>
    public static T? Deserialize<T>(byte[] data, ProtoOptions? options = null)
    {
        return (T?)DeserializeCore(data, options);
    }

    /// <summary>
    /// Deserializes protobuf-encoded <paramref name="data"/>.
    /// </summary>
    /// <param name="data">The protobuf-encoded representation to deserialize.</param>
    /// <param name="options">Protobuf serializer options.</param>
    /// <returns>The deserialized graph.</returns>
    public static object? Deserialize(byte[] data, ProtoOptions? options = null)
    {
        return DeserializeCore(data, options);
    }

    /// <summary>
    /// Deserializes a graph of type <typeparamref name="T"/> from the given <paramref name="stream"/>.
    /// </summary>
    /// <typeparam name="T">The expected type of the deserialized graph.</typeparam>
    /// <param name="stream">The source stream.</param>
    /// <param name="options">Protobuf serializer options.</param>
    /// <returns>The deserialized graph.</returns>
    public static T? Deserialize<T>(Stream stream, ProtoOptions? options = null)
    {
        return (T?)DeserializeCore(stream, options);
    }

    /// <summary>
    /// Deserializes a graph from the given <paramref name="stream"/>.
    /// </summary>
    /// <param name="stream">The source stream.</param>
    /// <param name="options">Protobuf serializer options.</param>
    /// <returns>The deserialized graph.</returns>
    public static object? Deserialize(Stream stream, ProtoOptions? options = null)
    {
        return DeserializeCore(stream, options);
    }

    /// <summary>
    /// Deserializes a graph of type <typeparamref name="T"/> from the given <paramref name="data"/>.
    /// </summary>
    /// <typeparam name="T">The expected type of the deserialized graph.</typeparam>
    /// <param name="data">The source stream.</param>
    /// <param name="options">Protobuf serializer options.</param>
    /// <returns>The deserialized graph.</returns>
    public static T? Deserialize<T>(ReadOnlySequence<byte> data, ProtoOptions? options = null)
    {
        return (T?)DeserializeCore(data, options);
    }

    /// <summary>
    /// Deserializes a graph from the given <paramref name="data"/>.
    /// </summary>
    /// <param name="data">The source stream.</param>
    /// <param name="options">Protobuf serializer options.</param>
    /// <returns>The deserialized graph.</returns>
    public static object? Deserialize(ReadOnlySequence<byte> data, ProtoOptions? options = null)
    {
        return DeserializeCore(data, options);
    }

    /// <summary>
    /// Deserializes a graph of type <typeparamref name="T"/> from the given <paramref name="data"/>.
    /// </summary>
    /// <typeparam name="T">The expected type of the deserialized graph.</typeparam>
    /// <param name="data">The source stream.</param>
    /// <param name="options">Protobuf serializer options.</param>
    /// <returns>The deserialized graph.</returns>
    public static T? Deserialize<T>(ReadOnlySpan<byte> data, ProtoOptions? options = null)
    {
        return (T?)DeserializeCore(data, options);
    }

    /// <summary>
    /// Deserializes a graph from the given <paramref name="data"/>.
    /// </summary>
    /// <param name="data">The source stream.</param>
    /// <param name="options">Protobuf serializer options.</param>
    /// <returns>The deserialized graph.</returns>
    public static object? Deserialize(ReadOnlySpan<byte> data, ProtoOptions? options = null)
    {
        return DeserializeCore(data, options);
    }

    private static object? DeserializeCore(ReadOnlySpan<byte> data, ProtoOptions? options = null)
    {
        return FromProto(Proto.Value.Parser.ParseFrom(data), options);
    }

    private static object? DeserializeCore(ReadOnlySequence<byte> data, ProtoOptions? options = null)
    {
        return FromProto(Proto.Value.Parser.ParseFrom(data), options);
    }

    private static object? DeserializeCore(Stream stream, ProtoOptions? options = null)
    {
        stream.AssertNotNull();
        return FromProto(Proto.Value.Parser.ParseFrom(stream), options);
    }

    private static object? DeserializeCore(byte[] data, ProtoOptions? options = null)
    {
        data.AssertNotNull();
        return FromProto(Proto.Value.Parser.ParseFrom(data), options);
    }

    private static Proto.Value ToProto(object? graph, ProtoOptions? options = null)
    {
        var context = new ProtoContext(options);
        return ValueMapper.Instance.ToProto(graph, context);
    }

    private static object? FromProto(Proto.Value proto, ProtoOptions? options = null)
    {
        var context = new ProtoContext(options);
        return ValueMapper.Instance.FromProto(proto, context);
    }
}
