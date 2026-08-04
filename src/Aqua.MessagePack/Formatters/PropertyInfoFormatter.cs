// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Formatters;

/// <summary>
/// MessagePack formatter for <see cref="PropertyInfo"/>.
/// </summary>
/// <remarks>
/// Encodes as a fixed-length array matching the declared <c>[DataMember]</c> order:
/// <c>[Name, DeclaringType, IsStatic, PropertyType]</c>.
/// </remarks>
public sealed class PropertyInfoFormatter : IMessagePackFormatter<PropertyInfo?>
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="PropertyInfoFormatter"/>.
    /// </summary>
    public static readonly PropertyInfoFormatter Instance = new();

    private const int FieldCount = 4;

    /// <inheritdoc/>
    public void Serialize(ref MessagePackWriter writer, PropertyInfo? value, MessagePackSerializerOptions options)
    {
        writer.SerializeReferenceValue(FieldCount, value, options, SerializeMembers);

        static void SerializeMembers(ref MessagePackWriter writer, PropertyInfo value, MessagePackSerializerOptions options)
        {
            writer.Write(value.Name);

            TypeInfoFormatter.Instance.Serialize(ref writer, value.DeclaringType, options);

            if (value.IsStatic.HasValue)
            {
                writer.Write(value.IsStatic.Value);
            }
            else
            {
                writer.WriteNil();
            }

            TypeInfoFormatter.Instance.Serialize(ref writer, value.PropertyType, options);
        }
    }

    /// <inheritdoc/>
    public PropertyInfo? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        return reader.DeserializeReferenceValue<PropertyInfo>(options, DeserializeMembers);

        static void DeserializeMembers(ref MessagePackReader reader, uint fieldCount, PropertyInfo value, MessagePackSerializerOptions options)
        {
            value.Name = fieldCount > 0 ? reader.ReadString() : null;

            value.DeclaringType = fieldCount > 2 ? TypeInfoFormatter.Instance.Deserialize(ref reader, options) : null;

            value.IsStatic = fieldCount > 1 && !reader.TryReadNil() ? reader.ReadBoolean() : null;

            value.PropertyType = fieldCount > 3 ? TypeInfoFormatter.Instance.Deserialize(ref reader, options) : null;

            for (var i = FieldCount; i < fieldCount; i++)
            {
                reader.Skip();
            }
        }
    }
}
