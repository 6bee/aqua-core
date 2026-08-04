// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Formatters;

/// <summary>
/// MessagePack formatter for <see cref="TypeInfo"/>.
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

    private const uint FieldCount = 7;

    /// <inheritdoc/>
    public void Serialize(ref MessagePackWriter writer, TypeInfo? value, MessagePackSerializerOptions options)
    {
        writer.SerializeReferenceValue(FieldCount, value, options, SerializeMembers);

        static void SerializeMembers(ref MessagePackWriter writer, TypeInfo value, MessagePackSerializerOptions options)
        {
            writer.Write(value.Name);
            writer.Write(value.Namespace);
            TypeInfoFormatter.Instance.Serialize(ref writer, value.DeclaringType, options);
            WriteTypeInfoList(ref writer, value.GenericArguments, options);
            writer.Write(value.IsAnonymousType);
            writer.Write(value.IsGenericType);
            WritePropertyInfoList(ref writer, value.Properties, options);

            static void WriteTypeInfoList(ref MessagePackWriter writer, List<TypeInfo>? list, MessagePackSerializerOptions options)
            {
                if (list is null)
                {
                    writer.WriteNil();
                    return;
                }

                writer.WriteArrayHeader(list.Count);
                foreach (var item in list)
                {
                    TypeInfoFormatter.Instance.Serialize(ref writer, item, options);
                }
            }

            static void WritePropertyInfoList(ref MessagePackWriter writer, List<PropertyInfo>? list, MessagePackSerializerOptions options)
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
        }
    }

    /// <inheritdoc/>
    public TypeInfo? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        return reader.DeserializeReferenceValue<TypeInfo>(options, DeserializeMembers);

        static void DeserializeMembers(ref MessagePackReader reader, uint fieldCount, TypeInfo value, MessagePackSerializerOptions options)
        {
            value.Name = fieldCount > 0 ? reader.ReadString() : null;
            value.Namespace = fieldCount > 1 ? reader.ReadString() : null;
            value.DeclaringType = fieldCount > 2 ? TypeInfoFormatter.Instance.Deserialize(ref reader, options) : null;
            value.GenericArguments = fieldCount > 3 ? ReadTypeInfoList(ref reader, options) : null;
            value.IsAnonymousType = fieldCount > 4 && reader.ReadBoolean();
            value.IsGenericType = fieldCount > 5 && reader.ReadBoolean();
            value.Properties = fieldCount > 6 ? ReadPropertyInfoList(ref reader, options) : null;

            for (var i = FieldCount; i < fieldCount; i++)
            {
                reader.Skip();
            }

            static List<TypeInfo>? ReadTypeInfoList(ref MessagePackReader reader, MessagePackSerializerOptions options)
            {
                if (reader.TryReadNil())
                {
                    return null;
                }

                var count = reader.ReadArrayHeader();
                var list = new List<TypeInfo>(count);
                for (var i = 0; i < count; i++)
                {
                    var item = TypeInfoFormatter.Instance.Deserialize(ref reader, options);
                    if (item is not null)
                    {
                        list.Add(item);
                    }
                }

                return list;
            }


            static List<PropertyInfo>? ReadPropertyInfoList(ref MessagePackReader reader, MessagePackSerializerOptions options)
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
    }
}
