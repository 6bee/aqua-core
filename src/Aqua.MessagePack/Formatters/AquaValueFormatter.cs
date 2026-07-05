// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Formatters;

using Aqua.TypeExtensions;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Hand-written MessagePack formatter for the Aqua leaf-value union (<see cref="object"/>?).
/// </summary>
/// <remarks>
/// <para>
/// Every value is written as a 2-element array <c>[tag, payload]</c> so the stream is fully
/// self-describing and type-safe. Supported tags:
/// </para>
/// <list type="bullet">
/// <item><description><c>0</c> Null.</description></item>
/// <item><description><c>1</c> String.</description></item>
/// <item><description><c>2</c> Scalar — payload <c>[type_key, bin]</c> where <c>bin</c> is the
/// scalar encoded via <see cref="AquaScalarCodec"/>.</description></item>
/// <item><description><c>3</c> PackedArray — payload <c>[element_type_key, bin]</c> (little-endian)
/// for eligible fixed-width primitive arrays and <c>byte[]</c>.</description></item>
/// <item><description><c>4</c> Collection — payload is a MessagePack array of leaf values
/// (used for ineligible/null-bearing arrays and lists).</description></item>
/// <item><description><c>5</c> DynamicObject.</description></item>
/// <item><description><c>6</c> Property.</description></item>
/// <item><description><c>7</c> PropertySet.</description></item>
/// <item><description><c>8</c> TypeInfo.</description></item>
/// </list>
/// </remarks>
public sealed class AquaValueFormatter : IMessagePackFormatter<object?>
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="AquaValueFormatter"/>.
    /// </summary>
    public static readonly AquaValueFormatter Instance = new();

    /// <inheritdoc/>
    public void Serialize(ref MessagePackWriter writer, object? value, MessagePackSerializerOptions options)
    {
        switch (value)
        {
            case null:
                writer.WriteArrayHeader(1);
                writer.Write((byte)ValueTag.Null);
                return;

            case string s:
                writer.WriteArrayHeader(2);
                writer.Write((byte)ValueTag.String);
                writer.Write(s);
                return;

            case DynamicObject dynamicObject:
                WriteTagged(ref writer, ValueTag.DynamicObject, dynamicObject, options.Resolver.GetFormatterWithVerify<DynamicObject?>(), options);
                return;

            case PropertySet propertySet:
                WriteTagged(ref writer, ValueTag.PropertySet, propertySet, options.Resolver.GetFormatterWithVerify<PropertySet?>(), options);
                return;

            case Property property:
                WriteTagged(ref writer, ValueTag.Property, property, options.Resolver.GetFormatterWithVerify<Property?>(), options);
                return;

            case TypeInfo typeInfo:
                WriteTagged(ref writer, ValueTag.TypeInfo, typeInfo, options.Resolver.GetFormatterWithVerify<TypeInfo?>(), options);
                return;

            ////case Array array:
            ////    WriteArray(ref writer, array, options);
            ////    return;

            case IEnumerable enumerable when value is not IFormattable:
                WriteCollection(ref writer, enumerable, options);
                return;

            default:
                WriteScalar(ref writer, value);
                return;
        }
    }

    /// <inheritdoc/>
    public object? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        options.Security.DepthStep(ref reader);
        try
        {
            var count = reader.ReadArrayHeader();
            if (count == 0)
            {
                return null;
            }

            var tag = (ValueTag)reader.ReadByte();
            object? result = tag switch
            {
                ValueTag.Null => null,
                ValueTag.String => count > 1 ? reader.ReadString() : null,
                ValueTag.Scalar => ReadScalar(ref reader),
                ValueTag.PackedArray => throw SerializationException("Packed array are not supported"),
                ////ValueTag.PackedArray => ReadPackedArray(ref reader),
                ValueTag.Collection => ReadCollection(ref reader, options),
                ValueTag.DynamicObject => options.Resolver.GetFormatterWithVerify<DynamicObject?>().Deserialize(ref reader, options),
                ValueTag.Property => options.Resolver.GetFormatterWithVerify<Property?>().Deserialize(ref reader, options),
                ValueTag.PropertySet => options.Resolver.GetFormatterWithVerify<PropertySet?>().Deserialize(ref reader, options),
                ValueTag.TypeInfo => options.Resolver.GetFormatterWithVerify<TypeInfo?>().Deserialize(ref reader, options),
                _ => throw SerializationException($"Unknown Aqua value tag '{tag}'."),
            };

            var skip = tag == ValueTag.Null ? 1 : 2;
            for (var i = skip; i < count; i++)
            {
                reader.Skip();
            }

            return result;
        }
        finally
        {
            reader.Depth--;
        }
    }

    private static void WriteTagged<T>(ref MessagePackWriter writer, ValueTag tag, T value, IMessagePackFormatter<T> formatter, MessagePackSerializerOptions options)
    {
        writer.WriteArrayHeader(2);
        writer.Write((byte)tag);
        formatter.Serialize(ref writer, value, options);
    }

    ////private void WriteArray(ref MessagePackWriter writer, Array array, MessagePackSerializerOptions options)
    ////{
    ////    if (_resolver.TryPackArray(array, out var elementTypeKey, out var bytes))
    ////    {
    ////        writer.WriteArrayHeader(2);
    ////        writer.Write((byte)ValueTag.PackedArray);
    ////        writer.WriteArrayHeader(2);
    ////        writer.Write(elementTypeKey);
    ////        writer.Write(bytes);
    ////        return;
    ////    }

    ////    WriteCollection(ref writer, array, options);
    ////}

    private void WriteCollection(ref MessagePackWriter writer, IEnumerable items, MessagePackSerializerOptions options)
    {
        var buffer = new List<object?>();
        foreach (var item in items)
        {
            buffer.Add(item);
        }

        writer.WriteArrayHeader(2);
        writer.Write((byte)ValueTag.Collection);
        writer.WriteArrayHeader(buffer.Count);
        foreach (var item in buffer)
        {
            Serialize(ref writer, item, options);
        }
    }

    private object?[] ReadCollection(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var count = reader.ReadArrayHeader();
        var result = new object?[count];
        for (var i = 0; i < count; i++)
        {
            result[i] = Deserialize(ref reader, options);
        }

        return result;
    }

    private static void WriteScalar(ref MessagePackWriter writer, object value)
    {
        var type = value.GetType();
        var dataType = DataType.FromType(type) ?? throw SerializationException($"Value of type {type.GetFriendlyName()} is not supported.");

        writer.WriteArrayHeader(2);
        writer.Write((byte)ValueTag.Scalar);
        writer.WriteArrayHeader(2);
        writer.Write((byte)dataType);
        AquaScalarCodec.Write(ref writer, value);
    }

    private static object ReadScalar(ref MessagePackReader reader)
    {
        var count = reader.ReadArrayHeader();
        var dataType = (DataType)reader.ReadByte();
        var type = DataType.ToType(dataType) ?? throw SerializationException($"Unsupported data type {dataType}.");
        var value = AquaScalarCodec.Read(ref reader, type);

        for (var i = 2; i < count; i++)
        {
            reader.Skip();
        }

        return value;
    }

    ////private object ReadPackedArray(ref MessagePackReader reader)
    ////{
    ////    var count = reader.ReadArrayHeader();
    ////    var elementTypeKey = reader.ReadString() ?? throw SerializationException("Missing packed-array element type key.");
    ////    var bytes = ReadByteArray(ref reader);
    ////    var array = _resolver.UnpackArray(elementTypeKey, bytes);

    ////    for (var i = 2; i < count; i++)
    ////    {
    ////        reader.Skip();
    ////    }

    ////    return array;
    ////}

    ////private static byte[] ReadByteArray(ref MessagePackReader reader)
    ////{
    ////    var sequence = reader.ReadBytes();
    ////    return sequence.HasValue ? sequence.Value.ToArray() : [];
    ////}

    private static MessagePackSerializationException SerializationException(string message) => new(message);
}
