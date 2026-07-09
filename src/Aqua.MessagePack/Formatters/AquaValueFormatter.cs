// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Formatters;

using Aqua.TypeExtensions;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

/// <summary>
/// MessagePack formatter for the Aqua leaf-value union (<see cref="object"/>?).
/// </summary>
/// <remarks>
/// <para>
/// Every value is written as a 2-element array <c>[tag, payload]</c> so the stream is fully
/// self-describing and type-safe. Supported tags:
/// </para>
/// <list type="bullet">
/// <item><description><c>0</c> Null.</description></item>
/// <item><description><c>1</c> String.</description></item>
/// <item><description><c>2</c> Scalar — payload <c>[type_key, msgpack]</c>.</description></item>
/// <item><description><c>3</c> PackedArray — payload <c>[element_type_key, msgpack]</c>
/// for eligible fixed-width primitive arrays.</description></item>
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
                writer.Write((byte)ValueKind.Null);
                return;

            case string s:
                writer.WriteArrayHeader(2);
                writer.Write((byte)ValueKind.String);
                writer.Write(s);
                return;

            case DynamicObject dynamicObject:
                WriteTagged(ref writer, ValueKind.DynamicObject, dynamicObject, options.Resolver.GetFormatterWithVerify<DynamicObject?>(), options);
                return;

            case PropertySet propertySet:
                WriteTagged(ref writer, ValueKind.PropertySet, propertySet, options.Resolver.GetFormatterWithVerify<PropertySet?>(), options);
                return;

            case Property property:
                WriteTagged(ref writer, ValueKind.Property, property, options.Resolver.GetFormatterWithVerify<Property?>(), options);
                return;

            case TypeInfo typeInfo:
                WriteTagged(ref writer, ValueKind.TypeInfo, typeInfo, options.Resolver.GetFormatterWithVerify<TypeInfo?>(), options);
                return;

            ////case Array array:
            ////    WriteArray(ref writer, array, options);
            ////    return;

            case IEnumerable enumerable when value is not IFormattable:
                WriteCollection(ref writer, enumerable, options);
                return;

            default:
                WriteScalar(ref writer, value, options);
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

            var tag = (ValueKind)reader.ReadByte();
            object? result = tag switch
            {
                ValueKind.Null => null,
                ValueKind.String => count > 1 ? reader.ReadString() : null,
                ValueKind.Scalar => ReadScalar(ref reader, options),
                ValueKind.PackedArray => throw SerializationException("Packed array are not supported"),
                ////ValueTag.PackedArray => ReadPackedArray(ref reader),
                ValueKind.Collection => ReadCollection(ref reader, options),
                ValueKind.DynamicObject => options.Resolver.GetFormatterWithVerify<DynamicObject?>().Deserialize(ref reader, options),
                ValueKind.Property => options.Resolver.GetFormatterWithVerify<Property?>().Deserialize(ref reader, options),
                ValueKind.PropertySet => options.Resolver.GetFormatterWithVerify<PropertySet?>().Deserialize(ref reader, options),
                ValueKind.TypeInfo => options.Resolver.GetFormatterWithVerify<TypeInfo?>().Deserialize(ref reader, options),
                _ => throw SerializationException($"Unknown Aqua value tag '{tag}'."),
            };

            var skip = tag == ValueKind.Null ? 1 : 2;
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

    private static void WriteTagged<T>(ref MessagePackWriter writer, ValueKind kind, T value, IMessagePackFormatter<T> formatter, MessagePackSerializerOptions options)
    {
        writer.WriteArrayHeader(2);
        writer.Write((byte)kind);
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
        writer.Write((byte)ValueKind.Collection);
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

    private static void WriteScalar(ref MessagePackWriter writer, object value, MessagePackSerializerOptions options)
    {
        var type = value.GetType();
        var dataType = DataType.FromType(type) ?? throw SerializationException($"Value of type {type.GetFriendlyName()} is not supported.");

        writer.WriteArrayHeader(2);
        writer.Write((byte)ValueKind.Scalar);
        writer.WriteArrayHeader(2);
        writer.Write((byte)dataType);
        switch (dataType)
        {
            case DataType.Bool: writer.Write((bool)value); return;
            case DataType.UInt8: writer.Write((byte)value); return;
            case DataType.Int8: writer.Write((sbyte)value); return;
            case DataType.Int16: writer.Write((short)value); return;
            case DataType.UInt16: writer.Write((ushort)value); return;
            case DataType.Int32: writer.Write((int)value); return;
            case DataType.UInt32: writer.Write((uint)value); return;
            case DataType.Int64: writer.Write((long)value); return;
            case DataType.UInt64: writer.Write((ulong)value); return;
            case DataType.Float32: writer.Write((float)value); return;
            case DataType.Float64: writer.Write((double)value); return;
            case DataType.Char: writer.Write((char)value); return;
            case DataType.Decimal: Serialize<decimal>(ref writer, value, options); return;
            case DataType.Uuid: Serialize<Guid>(ref writer, value, options); return;
            case DataType.DateTime: Serialize<DateTime>(ref writer, value, options); return;
            case DataType.DateTimeOffset: Serialize<DateTimeOffset>(ref writer, value, options); return;
            case DataType.TimeSpan: Serialize<TimeSpan>(ref writer, value, options); return;
            case DataType.BigInteger: Serialize<BigInteger>(ref writer, value, options); return;
            case DataType.Complex128: Serialize<Complex>(ref writer, value, options); return;
#if NET5_0_OR_GREATER
            case DataType.Float16: Serialize<Half>(ref writer, value, options); return;
#endif // NET5_0_OR_GREATER
#if NET6_0_OR_GREATER
            case DataType.DateOnly: Serialize<DateOnly>(ref writer, value, options); return;
            case DataType.TimeOnly: Serialize<TimeOnly>(ref writer, value, options); return;
#endif // NET6_0_OR_GREATER
#if NET7_0_OR_GREATER
            case DataType.Int128: Serialize<Int128>(ref writer, value, options); return;
            case DataType.UInt128: Serialize<UInt128>(ref writer, value, options); return;
#endif // NET7_0_OR_GREATER
            default: throw SerializationException($"Value of type {value.GetType().GetFriendlyName()} is not supported.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void Serialize<T>(ref MessagePackWriter writer, object value, MessagePackSerializerOptions options)
            => options.Resolver.GetFormatterWithVerify<T>().Serialize(ref writer, (T)value, options);
    }

    private static object ReadScalar(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var count = reader.ReadArrayHeader();
        var dataType = (DataType)reader.ReadByte();
        object value = dataType switch
        {
            DataType.UInt8 => reader.ReadByte(),
            DataType.Int8 => reader.ReadSByte(),
            DataType.Int16 => reader.ReadInt16(),
            DataType.UInt16 => reader.ReadUInt16(),
            DataType.Int32 => reader.ReadInt32(),
            DataType.UInt32 => reader.ReadUInt32(),
            DataType.Int64 => reader.ReadInt64(),
            DataType.UInt64 => reader.ReadUInt64(),
#if NET7_0_OR_GREATER
            DataType.Int128 => Deserialize<Int128>(ref reader, options),
            DataType.UInt128 => Deserialize<UInt128>(ref reader, options),
#endif
#if NET5_0_OR_GREATER
            DataType.Float16 => Deserialize<Half>(ref reader, options),
#endif
            DataType.Float32 => reader.ReadSingle(),
            DataType.Float64 => reader.ReadDouble(),
            DataType.Bool => reader.ReadBoolean(),
            DataType.Char => reader.ReadChar(),
            DataType.BigInteger => Deserialize<BigInteger>(ref reader, options),
            DataType.Complex128 => Deserialize<Complex>(ref reader, options),
            DataType.Decimal => Deserialize<decimal>(ref reader, options),
            DataType.Uuid => Deserialize<Guid>(ref reader, options),
            DataType.DateTime => Deserialize<DateTime>(ref reader, options),
            DataType.DateTimeOffset => Deserialize<DateTimeOffset>(ref reader, options),
            DataType.TimeSpan => Deserialize<TimeSpan>(ref reader, options),
#if NET6_0_OR_GREATER
            DataType.DateOnly => Deserialize<DateOnly>(ref reader, options),
            DataType.TimeOnly => Deserialize<TimeOnly>(ref reader, options),
#endif
            _ => throw SerializationException($"Unsupported data type {dataType}."),
        };

        for (var i = 2; i < count; i++)
        {
            reader.Skip();
        }

        return value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static T Deserialize<T>(ref MessagePackReader reader, MessagePackSerializerOptions options)
            => options.Resolver.GetFormatterWithVerify<T>().Deserialize(ref reader, options);
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
