// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Formatters;

/// <summary>
/// MessagePack formatter for <see cref="DynamicObject"/>.
/// </summary>
/// <remarks>
/// Encodes as a fixed-length array <c>[Type, Properties]</c> matching the declared
/// <c>[DataMember]</c> order. A <see langword="null"/> <see cref="DynamicObject.Properties"/>
/// (i.e. <see cref="DynamicObject.IsNull"/>) is preserved by writing <c>nil</c> for the
/// properties slot.
/// </remarks>
public sealed class DynamicObjectFormatter : IMessagePackFormatter<DynamicObject?>
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="DynamicObjectFormatter"/>.
    /// </summary>
    public static readonly DynamicObjectFormatter Instance = new();

    private const int FieldCount = 2;

    /// <inheritdoc/>
    public void Serialize(ref MessagePackWriter writer, DynamicObject? value, MessagePackSerializerOptions options)
    {
        writer.SerializeReferenceValue(FieldCount, value, options, SerializeMembers);

        static void SerializeMembers(ref MessagePackWriter writer, DynamicObject value, MessagePackSerializerOptions options)
        {
            TypeInfoFormatter.Instance.Serialize(ref writer, value.Type, options);
            PropertySetFormatter.Instance.Serialize(ref writer, value.Properties, options);
        }
    }

    /// <inheritdoc/>
    public DynamicObject? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        return reader.DeserializeReferenceValue<DynamicObject>(options, DeserializeMembers);

        static void DeserializeMembers(ref MessagePackReader reader, uint fieldCount, DynamicObject value, MessagePackSerializerOptions options)
        {
            value.Type = fieldCount > 0 ? TypeInfoFormatter.Instance.Deserialize(ref reader, options) : null;
            value.Properties = fieldCount > 1 ? PropertySetFormatter.Instance.Deserialize(ref reader, options) : null;

            for (var i = FieldCount; i < fieldCount; i++)
            {
                reader.Skip();
            }
        }
    }
}
