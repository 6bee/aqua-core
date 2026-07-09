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
///   Every value is written as a 2-element array <c>[tag, payload]</c> so the stream is fully
///   self-describing and type-safe.
/// </para>
/// <para>
///   Supported tags:
///   <list type="bullet">
///     <item><description><c>1</c> Null.</description></item>
///     <item><description><c>2</c> String.</description></item>
///     <item><description><c>3</c> Scalar — payload <c>[type_key, msgpack]</c>.</description></item>
///     <item><description><c>4</c> PackedArray — payload <c>[element_type_key, msgpack]</c>
///     for eligible fixed-width primitive arrays.</description></item>
///     <item><description><c>5</c> Collection — payload is a msgpack array of leaf values
///     (used for ineligible/null-bearing arrays and lists).</description></item>
///     <item><description><c>80</c> DynamicObject.</description></item>
///     <item><description><c>81</c> Property.</description></item>
///     <item><description><c>82</c> PropertySet.</description></item>
///     <item><description><c>83</c> TypeInfo.</description></item>
///   </list>
/// </para>
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

            case Array array:
                WriteArray(ref writer, array, options);
                return;

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

            var kind = (ValueKind)reader.ReadByte();
            object? result = kind switch
            {
                ValueKind.Null => null,
                ValueKind.String => count > 1 ? reader.ReadString() : null,
                ValueKind.Scalar => ReadScalar(ref reader, options),
                ValueKind.PackedArray => ReadPackedArray(ref reader, options),
                ValueKind.Collection => ReadCollection(ref reader, options),
                ValueKind.DynamicObject => options.Resolver.GetFormatterWithVerify<DynamicObject?>().Deserialize(ref reader, options),
                ValueKind.Property => options.Resolver.GetFormatterWithVerify<Property?>().Deserialize(ref reader, options),
                ValueKind.PropertySet => options.Resolver.GetFormatterWithVerify<PropertySet?>().Deserialize(ref reader, options),
                ValueKind.TypeInfo => options.Resolver.GetFormatterWithVerify<TypeInfo?>().Deserialize(ref reader, options),
                _ => throw SerializationException($"Unknown Aqua value tag '{kind}'."),
            };

            var skip = kind is ValueKind.Null ? 1 : 2;
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

    private void WriteArray(ref MessagePackWriter writer, Array array, MessagePackSerializerOptions options)
    {
        var elementType = array.GetType().GetElementType()!;
        if (DataType.FromType(elementType) is { } dataType)
        {
            var success = dataType switch
            {
                DataType.Bool => Serialize<bool>(ref writer, dataType, array, options),
                DataType.UInt8 => Serialize<byte>(ref writer, dataType, array, options),
                DataType.Int8 => Serialize<sbyte>(ref writer, dataType, array, options),
                DataType.Int16 => Serialize<short>(ref writer, dataType, array, options),
                DataType.UInt16 => Serialize<ushort>(ref writer, dataType, array, options),
                DataType.Int32 => Serialize<int>(ref writer, dataType, array, options),
                DataType.UInt32 => Serialize<uint>(ref writer, dataType, array, options),
                DataType.Int64 => Serialize<long>(ref writer, dataType, array, options),
                DataType.UInt64 => Serialize<ulong>(ref writer, dataType, array, options),
#if NET7_0_OR_GREATER
                DataType.Int128 => Serialize<Int128>(ref writer, dataType, array, options),
                DataType.UInt128 => Serialize<UInt128>(ref writer, dataType, array, options),
#endif // NET7_0_OR_GREATER
#if NET5_0_OR_GREATER
                DataType.Float16 => Serialize<Half>(ref writer, dataType, array, options),
#endif // NET5_0_OR_GREATER
                DataType.Float32 => Serialize<float>(ref writer, dataType, array, options),
                DataType.Float64 => Serialize<double>(ref writer, dataType, array, options),
                DataType.Char => Serialize<char>(ref writer, dataType, array, options),
                DataType.Decimal => Serialize<decimal>(ref writer, dataType, array, options),
                DataType.BigInteger => Serialize<BigInteger>(ref writer, dataType, array, options),
                DataType.Complex128 => Serialize<Complex>(ref writer, dataType, array, options),
                DataType.Uuid => Serialize<Guid>(ref writer, dataType, array, options),
                DataType.DateTime => Serialize<DateTime>(ref writer, dataType, array, options),
                DataType.DateTimeOffset => Serialize<DateTimeOffset>(ref writer, dataType, array, options),
                DataType.TimeSpan => Serialize<TimeSpan>(ref writer, dataType, array, options),
#if NET6_0_OR_GREATER
                DataType.DateOnly => Serialize<DateOnly>(ref writer, dataType, array, options),
                DataType.TimeOnly => Serialize<TimeOnly>(ref writer, dataType, array, options),
#endif // NET6_0_OR_GREATER
                _ => false, // unreachable
            };

            if (success)
            {
                return;
            }
        }

        WriteCollection(ref writer, array, options);

        static bool Serialize<T>(ref MessagePackWriter writer, DataType dataType, Array array, MessagePackSerializerOptions options)
        {
            if (options.Resolver.GetFormatter<T[]>() is { } formatter)
            {
                writer.WriteArrayHeader(2);
                writer.Write((byte)ValueKind.PackedArray);
                writer.WriteArrayHeader(2);
                writer.Write((byte)dataType);
                formatter.Serialize(ref writer, (T[])array, options);
                return true;
            }

            return false;
        }
    }

    private void WriteCollection(ref MessagePackWriter writer, IEnumerable items, MessagePackSerializerOptions options)
    {
        var collection = items as ICollection ?? AsCollection(items);

        writer.WriteArrayHeader(2);
        writer.Write((byte)ValueKind.Collection);
        writer.WriteArrayHeader(collection.Count);
        foreach (var item in collection)
        {
            Serialize(ref writer, item, options);
        }

        static ICollection AsCollection(IEnumerable items)
        {
            var collection = new List<object?>();
            foreach (var item in items)
            {
                collection.Add(item);
            }

            return collection;
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

    private object ReadPackedArray(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var count = reader.ReadArrayHeader();
        var dataType = (DataType)reader.ReadByte();
        object array = dataType switch
        {
            DataType.UInt8 => Deserialize<byte>(ref reader, options),
            DataType.Int8 => Deserialize<sbyte>(ref reader, options),
            DataType.Int16 => Deserialize<short>(ref reader, options),
            DataType.UInt16 => Deserialize<ushort>(ref reader, options),
            DataType.Int32 => Deserialize<int>(ref reader, options),
            DataType.UInt32 => Deserialize<uint>(ref reader, options),
            DataType.Int64 => Deserialize<long>(ref reader, options),
            DataType.UInt64 => Deserialize<ulong>(ref reader, options),
#if NET7_0_OR_GREATER
            DataType.Int128 => Deserialize<Int128>(ref reader, options),
            DataType.UInt128 => Deserialize<UInt128>(ref reader, options),
#endif
#if NET5_0_OR_GREATER
            DataType.Float16 => Deserialize<Half>(ref reader, options),
#endif
            DataType.Float32 => Deserialize<float>(ref reader, options),
            DataType.Float64 => Deserialize<double>(ref reader, options),
            DataType.Bool => Deserialize<bool>(ref reader, options),
            DataType.Char => Deserialize<char>(ref reader, options),
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
            _ => throw SerializationException($"Unsupported element type {dataType}."),
        };

        for (var i = 2; i < count; i++)
        {
            reader.Skip();
        }

        return array;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static T[] Deserialize<T>(ref MessagePackReader reader, MessagePackSerializerOptions options)
            => options.Resolver.GetFormatterWithVerify<T[]>().Deserialize(ref reader, options);
    }

    private static MessagePackSerializationException SerializationException(string message) => new(message);
}
