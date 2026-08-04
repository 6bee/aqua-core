// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Formatters;

/// <summary>
/// MessagePack formatter for <see cref="FieldInfo"/>.
/// </summary>
/// <remarks>
/// Encodes as a fixed-length array matching the declared <c>[DataMember]</c> order:
/// <c>[Name, IsStatic, DeclaringType]</c>.
/// </remarks>
public sealed class FieldInfoFormatter : IMessagePackFormatter<FieldInfo?>
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="FieldInfoFormatter"/>.
    /// </summary>
    public static readonly FieldInfoFormatter Instance = new();

    private const int FieldCount = 3;

    /// <inheritdoc/>
    public void Serialize(ref MessagePackWriter writer, FieldInfo? value, MessagePackSerializerOptions options)
    {
        writer.SerializeReferenceValue(FieldCount, value, options, SerializeMembers);

        static void SerializeMembers(ref MessagePackWriter writer, FieldInfo value, MessagePackSerializerOptions options)
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
        }
    }

    /// <inheritdoc/>
    public FieldInfo? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        return reader.DeserializeReferenceValue<FieldInfo>(options, DeserializeMembers);

        static void DeserializeMembers(ref MessagePackReader reader, uint fieldCount, FieldInfo value, MessagePackSerializerOptions options)
        {
            value.Name = fieldCount > 0 ? reader.ReadString() : null;

            value.IsStatic = fieldCount > 1 && !reader.TryReadNil() ? reader.ReadBoolean() : null;

            value.DeclaringType = fieldCount > 2 ? TypeInfoFormatter.Instance.Deserialize(ref reader, options) : null;

            for (var i = FieldCount; i < fieldCount; i++)
            {
                reader.Skip();
            }
        }
    }
}
