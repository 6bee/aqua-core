// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf;

using Google.Protobuf;
using System.IO;
using Proto = Aqua.Protobuf.Schema;

// TODO: consider circular reference support

/// <summary>
/// Schema-first Google.Protobuf serializer for <i>Aqua</i> types. Object graphs are mapped through a
/// recursive, self-describing protobuf value union.
/// </summary>
public static class AquaProtobufSerializer
{
    /// <summary>
    /// Serializes the specified <paramref name="graph"/> to a protobuf-encoded byte array.
    /// </summary>
    /// <typeparam name="T">The type of the graph to serialize.</typeparam>
    /// <param name="graph">The object graph to serialize.</param>
    /// <returns>The protobuf-encoded representation of <paramref name="graph"/>.</returns>
    public static byte[] Serialize<T>(T graph)
    {
        var mapper = new ProtobufValueMapper();
        return mapper.ToValue(graph).ToByteArray();
    }

    /// <summary>
    /// Serializes the specified <paramref name="graph"/> to the given <paramref name="stream"/>.
    /// </summary>
    /// <typeparam name="T">The type of the graph to serialize.</typeparam>
    /// <param name="graph">The object graph to serialize.</param>
    /// <param name="stream">The destination stream.</param>
    public static void Serialize<T>(T graph, Stream stream)
    {
        stream.AssertNotNull();
        var mapper = ProtobufValueMapper.Instance;
        mapper.ToValue(graph).WriteTo(stream);
    }

    /// <summary>
    /// Deserializes a graph of type <typeparamref name="T"/> from the protobuf-encoded <paramref name="data"/>.
    /// </summary>
    /// <typeparam name="T">The expected type of the deserialized graph.</typeparam>
    /// <param name="data">The protobuf-encoded representation to deserialize.</param>
    /// <returns>The deserialized graph.</returns>
    public static T Deserialize<T>(byte[] data)
    {
        data.AssertNotNull();
        var value = Proto.Value.Parser.ParseFrom(data);
        var mapper = ProtobufValueMapper.Instance;
        return (T)mapper.FromValue(value)!;
    }

    /// <summary>
    /// Deserializes protobuf-encoded <paramref name="data"/>.
    /// </summary>
    /// <param name="data">The protobuf-encoded representation to deserialize.</param>
    /// <returns>The deserialized graph.</returns>
    public static object Deserialize(byte[] data)
    {
        data.AssertNotNull();
        var value = Proto.Value.Parser.ParseFrom(data);
        var mapper = ProtobufValueMapper.Instance;
        return mapper.FromValue(value)!;
    }

    /// <summary>
    /// Deserializes a graph of type <typeparamref name="T"/> from the given <paramref name="stream"/>.
    /// </summary>
    /// <typeparam name="T">The expected type of the deserialized graph.</typeparam>
    /// <param name="stream">The source stream.</param>
    /// <returns>The deserialized graph.</returns>
    public static T Deserialize<T>(Stream stream)
    {
        stream.AssertNotNull();
        var value = Proto.Value.Parser.ParseFrom(stream);
        var mapper = ProtobufValueMapper.Instance;
        return (T)mapper.FromValue(value)!;
    }
}
