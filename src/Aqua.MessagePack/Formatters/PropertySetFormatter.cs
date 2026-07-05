// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Formatters;

/// <summary>
/// Hand-written MessagePack formatter for <see cref="PropertySet"/>.
/// </summary>
/// <remarks>
/// Encodes as a MessagePack array of <see cref="Property"/> values. A <see langword="null"/>
/// property set is written as <c>nil</c>.
/// </remarks>
public sealed class PropertySetFormatter : IMessagePackFormatter<PropertySet?>
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

        var propertyFormatter = options.Resolver.GetFormatterWithVerify<Property?>();
        writer.WriteArrayHeader(value.Count);
        foreach (var property in value)
        {
            propertyFormatter.Serialize(ref writer, property, options);
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
            var propertyFormatter = options.Resolver.GetFormatterWithVerify<Property?>();
            var count = reader.ReadArrayHeader();
            var properties = new List<Property>(count);
            for (var i = 0; i < count; i++)
            {
                var property = propertyFormatter.Deserialize(ref reader, options);
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
