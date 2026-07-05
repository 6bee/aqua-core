// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Formatters;

/// <summary>
/// Hand-written MessagePack formatter for <see cref="DynamicObject"/>.
/// </summary>
/// <remarks>
/// Encodes as a fixed-length array <c>[Type, Properties]</c> matching the declared
/// <c>[DataMember]</c> order. A <see langword="null"/> <see cref="DynamicObject.Properties"/>
/// (i.e. <see cref="DynamicObject.IsNull"/>) is preserved by writing <c>nil</c> for the
/// properties slot.
/// </remarks>
public sealed class DynamicObjectFormatter : IMessagePackFormatter<DynamicObject?>
{
    private const int FieldCount = 2;

    /// <inheritdoc/>
    public void Serialize(ref MessagePackWriter writer, DynamicObject? value, MessagePackSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNil();
            return;
        }

        writer.WriteArrayHeader(FieldCount);
        options.Resolver.GetFormatterWithVerify<TypeInfo?>().Serialize(ref writer, value.Type, options);
        options.Resolver.GetFormatterWithVerify<PropertySet?>().Serialize(ref writer, value.Properties, options);
    }

    /// <inheritdoc/>
    public DynamicObject? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        options.Security.DepthStep(ref reader);
        try
        {
            var count = reader.ReadArrayHeader();
            var type = count > 0 ? options.Resolver.GetFormatterWithVerify<TypeInfo?>().Deserialize(ref reader, options) : null;
            var properties = count > 1 ? options.Resolver.GetFormatterWithVerify<PropertySet?>().Deserialize(ref reader, options) : null;

            for (var i = FieldCount; i < count; i++)
            {
                reader.Skip();
            }

            return new DynamicObject(type, properties);
        }
        finally
        {
            reader.Depth--;
        }
    }
}
