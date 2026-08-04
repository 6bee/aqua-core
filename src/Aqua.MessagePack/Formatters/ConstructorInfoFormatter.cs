// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Formatters;

/// <summary>
/// MessagePack formatter for <see cref="ConstructorInfo"/>.
/// </summary>
/// <remarks>
/// Encodes as a fixed-length array matching the declared <c>[DataMember]</c> order:
/// <c>[Name, IsStatic, DeclaringType, GenericArgumentTypes, ParameterTypes]</c>.
/// </remarks>
public sealed class ConstructorInfoFormatter : IMessagePackFormatter<ConstructorInfo?>
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="ConstructorInfoFormatter"/>.
    /// </summary>
    public static readonly ConstructorInfoFormatter Instance = new();

    private const int FieldCount = 5;

    /// <inheritdoc/>
    public void Serialize(ref MessagePackWriter writer, ConstructorInfo? value, MessagePackSerializerOptions options)
    {
        writer.SerializeReferenceValue(FieldCount, value, options, SerializeMembers);

        static void SerializeMembers(ref MessagePackWriter writer, ConstructorInfo value, MessagePackSerializerOptions options)
        {
            writer.Write(value.Name);

            if (value.IsStatic.HasValue)
            {
                writer.Write(value.IsStatic.Value);
            }
            else
            {
                writer.WriteNil();
            }

            TypeInfoFormatter.Instance.Serialize(ref writer, value.DeclaringType, options);

            WriteTypeInfoList(ref writer, value.GenericArgumentTypes, options);

            WriteTypeInfoList(ref writer, value.ParameterTypes, options);

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
        }
    }

    /// <inheritdoc/>
    public ConstructorInfo? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        return reader.DeserializeReferenceValue<ConstructorInfo>(options, DeserializeMembers);

        static void DeserializeMembers(ref MessagePackReader reader, uint fieldCount, ConstructorInfo value, MessagePackSerializerOptions options)
        {
            value.Name = fieldCount > 0 ? reader.ReadString() : null;

            value.IsStatic = fieldCount > 1 && !reader.TryReadNil() ? reader.ReadBoolean() : null;

            value.DeclaringType = fieldCount > 2 ? TypeInfoFormatter.Instance.Deserialize(ref reader, options) : null;

            value.GenericArgumentTypes = fieldCount > 3 ? ReadTypeInfoList(ref reader, options) : null;

            value.ParameterTypes = fieldCount > 4 ? ReadTypeInfoList(ref reader, options) : null;

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
        }
    }
}
