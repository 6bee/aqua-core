// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Formatters;

/// <summary>
/// Hand-written MessagePack formatter for <see cref="PropertyInfo"/> as it appears in
/// <see cref="TypeInfo.Properties"/>.
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
        if (value is null)
        {
            writer.WriteNil();
            return;
        }

        writer.WriteArrayHeader(FieldCount);
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

    /// <inheritdoc/>
    public PropertyInfo? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        options.Security.DepthStep(ref reader);
        try
        {
            var count = reader.ReadArrayHeader();
            var name = count > 0 ? reader.ReadString() : null;
            var declaringType = count > 1 ? TypeInfoFormatter.Instance.Deserialize(ref reader, options) : null;
            bool? isStatic = null;
            if (count > 2)
            {
                isStatic = reader.TryReadNil() ? null : reader.ReadBoolean();
            }

            var propertyType = count > 3 ? TypeInfoFormatter.Instance.Deserialize(ref reader, options) : null;

            for (var i = FieldCount; i < count; i++)
            {
                reader.Skip();
            }

            return new PropertyInfo(name!, propertyType!, declaringType)
            {
                IsStatic = isStatic,
            };
        }
        finally
        {
            reader.Depth--;
        }
    }
}
