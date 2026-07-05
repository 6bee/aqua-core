// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Formatters;

/// <summary>
/// Hand-written MessagePack formatter for <see cref="TypeInfo"/>.
/// </summary>
/// <remarks>
/// Encodes the native <see cref="TypeInfo"/> graph as a fixed-length array matching the
/// declared <c>[DataMember]</c> order:
/// <c>[Name, Namespace, DeclaringType, GenericArguments, IsAnonymousType, IsGenericType, Properties]</c>.
/// A <see langword="null"/> <see cref="TypeInfo"/> is written as MessagePack <c>nil</c>.
/// </remarks>
public sealed class TypeInfoFormatter : IMessagePackFormatter<TypeInfo?>
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="TypeInfoFormatter"/>.
    /// </summary>
    public static readonly TypeInfoFormatter Instance = new();

    private const int FieldCount = 7;

    /// <inheritdoc/>
    public void Serialize(ref MessagePackWriter writer, TypeInfo? value, MessagePackSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNil();
            return;
        }

        writer.WriteArrayHeader(FieldCount);
        writer.Write(value.Name);
        writer.Write(value.Namespace);
        Serialize(ref writer, value.DeclaringType, options);
        WriteTypeInfoList(ref writer, value.GenericArguments, options);
        writer.Write(value.IsAnonymousType);
        writer.Write(value.IsGenericType);
        WritePropertyInfoList(ref writer, value.Properties, options);
    }

    /// <inheritdoc/>
    public TypeInfo? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        options.Security.DepthStep(ref reader);
        try
        {
            var count = reader.ReadArrayHeader();
            var result = new TypeInfo
            {
                Name = count > 0 ? reader.ReadString() : null,
                Namespace = count > 1 ? reader.ReadString() : null,
                DeclaringType = count > 2 ? Deserialize(ref reader, options) : null,
                GenericArguments = count > 3 ? ReadTypeInfoList(ref reader, options) : null,
                IsAnonymousType = count > 4 && reader.ReadBoolean(),
                IsGenericType = count > 5 && reader.ReadBoolean(),
                Properties = count > 6 ? ReadPropertyInfoList(ref reader, options) : null,
            };

            for (var i = FieldCount; i < count; i++)
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

    private static void WriteTypeInfoList(ref MessagePackWriter writer, List<TypeInfo>? list, MessagePackSerializerOptions options)
    {
        if (list is null)
        {
            writer.WriteNil();
            return;
        }

        writer.WriteArrayHeader(list.Count);
        foreach (var item in list)
        {
            Instance.Serialize(ref writer, item, options);
        }
    }

    private static List<TypeInfo>? ReadTypeInfoList(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        var count = reader.ReadArrayHeader();
        var list = new List<TypeInfo>(count);
        for (var i = 0; i < count; i++)
        {
            var item = Instance.Deserialize(ref reader, options);
            if (item is not null)
            {
                list.Add(item);
            }
        }

        return list;
    }

    private static void WritePropertyInfoList(ref MessagePackWriter writer, List<PropertyInfo>? list, MessagePackSerializerOptions options)
    {
        if (list is null)
        {
            writer.WriteNil();
            return;
        }

        writer.WriteArrayHeader(list.Count);
        foreach (var item in list)
        {
            PropertyInfoFormatter.Instance.Serialize(ref writer, item, options);
        }
    }

    private static List<PropertyInfo>? ReadPropertyInfoList(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        var count = reader.ReadArrayHeader();
        var list = new List<PropertyInfo>(count);
        for (var i = 0; i < count; i++)
        {
            var item = PropertyInfoFormatter.Instance.Deserialize(ref reader, options);
            if (item is not null)
            {
                list.Add(item);
            }
        }

        return list;
    }
}
