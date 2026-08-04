// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Formatters;

/// <summary>
/// MessagePack formatter for <see cref="PropertySet"/>.
/// </summary>
/// <remarks>
/// Encodes as a MessagePack array of <see cref="Property"/> values. A <see langword="null"/>
/// property set is written as <c>nil</c>.
/// </remarks>
internal sealed class PropertySetFormatter : IMessagePackFormatter<PropertySet?>
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="PropertySetFormatter"/>.
    /// </summary>
    public static readonly PropertySetFormatter Instance = new();

    /// <inheritdoc/>
    public void Serialize(ref MessagePackWriter writer, PropertySet? value, MessagePackSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNil();
            return;
        }

        writer.WriteArrayHeader(value.Count);

        foreach (var property in value)
        {
            PropertyFormatter.Instance.Serialize(ref writer, property, options);
        }
    }

    /// <inheritdoc/>
    public PropertySet? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        options.Security.DepthStep(ref reader);
        try
        {
            var count = reader.ReadArrayHeader();

            var properties = new List<Property>(count);
            for (var i = 0; i < count; i++)
            {
                var property = PropertyFormatter.Instance.Deserialize(ref reader, options);
                if (property is not null)
                {
                    properties.Add(property);
                }
            }

            return new(properties);
        }
        finally
        {
            reader.Depth--;
        }
    }
}
