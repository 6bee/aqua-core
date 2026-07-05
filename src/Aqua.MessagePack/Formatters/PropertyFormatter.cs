// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Formatters;

/// <summary>
/// Hand-written MessagePack formatter for <see cref="Property"/>.
/// </summary>
/// <remarks>
/// Encodes as a fixed-length array <c>[Name, Value]</c>, where <c>Value</c> is written via the
/// Aqua leaf-value union (see <see cref="AquaValueFormatter"/>).
/// </remarks>
public sealed class PropertyFormatter : IMessagePackFormatter<Property?>
{
    private const int FieldCount = 2;

    /// <inheritdoc/>
    public void Serialize(ref MessagePackWriter writer, Property? value, MessagePackSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNil();
            return;
        }

        writer.WriteArrayHeader(FieldCount);
        writer.Write(value.Name);
        var valueFormatter = options.Resolver.GetFormatterWithVerify<object?>();
        valueFormatter.Serialize(ref writer, value.Value, options);
    }

    /// <inheritdoc/>
    public Property? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
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
            object? value = null;
            if (count > 1)
            {
                value = options.Resolver.GetFormatterWithVerify<object?>().Deserialize(ref reader, options);
            }

            for (var i = FieldCount; i < count; i++)
            {
                reader.Skip();
            }

            return new Property(name!, value);
        }
        finally
        {
            reader.Depth--;
        }
    }
}
