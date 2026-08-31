// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Formatters;

using Aqua.TypeExtensions;
using Aqua.TypeSystem;

/// <summary>
/// MessagePack formatter for <see cref="MemberInfo"/> (discriminated union).
/// </summary>
/// <remarks>
/// Null is encoded as nil. Otherwise encodes as a 2-element array <c>[kind_byte, payload]</c>
/// where <c>kind_byte</c> is a <see cref="MemberKind"/> discriminator and <c>payload</c> is
/// delegated to the matching concrete formatter.
/// </remarks>
public sealed class MemberInfoFormatter : IMessagePackFormatter<MemberInfo?>
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="MemberInfoFormatter"/>.
    /// </summary>
    public static readonly MemberInfoFormatter Instance = new();

    /// <inheritdoc/>
    public void Serialize(ref MessagePackWriter writer, MemberInfo? value, MessagePackSerializerOptions options)
    {
        switch (value)
        {
            case null:
                writer.WriteNil();
                return;

            case PropertyInfo propertyInfo:
                writer.WriteArrayHeader(2);
                writer.Write((byte)MemberKind.Property);
                PropertyInfoFormatter.Instance.Serialize(ref writer, propertyInfo, options);
                return;

            case FieldInfo fieldInfo:
                writer.WriteArrayHeader(2);
                writer.Write((byte)MemberKind.Field);
                FieldInfoFormatter.Instance.Serialize(ref writer, fieldInfo, options);
                return;

            case MethodInfo methodInfo:
                writer.WriteArrayHeader(2);
                writer.Write((byte)MemberKind.Method);
                MethodInfoFormatter.Instance.Serialize(ref writer, methodInfo, options);
                return;

            case ConstructorInfo constructorInfo:
                writer.WriteArrayHeader(2);
                writer.Write((byte)MemberKind.Constructor);
                ConstructorInfoFormatter.Instance.Serialize(ref writer, constructorInfo, options);
                return;

            default:
                throw new MessagePackSerializationException($"MemberInfo type {value.GetType().GetFriendlyName()} is not supported.");
        }
    }

    /// <inheritdoc/>
    public MemberInfo? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        options.Security.DepthStep(ref reader);
        try
        {
            var count = reader.ReadArrayHeader();
            if (count < 1)
            {
                throw new MessagePackSerializationException("Expected at least 1 element for MemberInfo discriminator.");
            }

            var kind = (MemberKind)reader.ReadByte();

            MemberInfo? result = kind switch
            {
                MemberKind.Property => PropertyInfoFormatter.Instance.Deserialize(ref reader, options),
                MemberKind.Field => FieldInfoFormatter.Instance.Deserialize(ref reader, options),
                MemberKind.Method => MethodInfoFormatter.Instance.Deserialize(ref reader, options),
                MemberKind.Constructor => ConstructorInfoFormatter.Instance.Deserialize(ref reader, options),
                _ => throw new MessagePackSerializationException($"Unknown MemberInfo kind '{kind}'."),
            };

            for (var i = 2; i < count; i++)
            {
                reader.Skip();
            }

            return result;
        }
        finally
        {
            reader.Depth--;
        }
    }

    private enum MemberKind : byte
    {
        Property = 1,
        Field = 2,
        Method = 3,
        Constructor = 4,
    }
}
