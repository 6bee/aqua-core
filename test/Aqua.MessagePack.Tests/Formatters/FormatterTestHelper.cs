// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Tests.Formatters;

using Aqua.MessagePack;
using global::MessagePack;
using global::MessagePack.Formatters;
using System.Buffers;

internal static class FormatterTestHelper
{
    public static MessagePackSerializerOptions Options(ReferenceHandler referenceHandler = ReferenceHandler.Unspecified)
        => MessagePackSerializerOptions.Standard
        .ConfigureAqua()
        .With(referenceHandler);

    public static ArrayBufferWriter<byte> Encode<T>(IMessagePackFormatter<T> formatter, T value, MessagePackSerializerOptions options = null)
    {
        options ??= Options();
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new MessagePackWriter(buffer);
        formatter.Serialize(ref writer, value, options);
        writer.Flush();
        return buffer;
    }

    public static T Decode<T>(IMessagePackFormatter<T> formatter, ReadOnlyMemory<byte> buffer, MessagePackSerializerOptions options = null)
    {
        options ??= Options();
        var reader = new MessagePackReader(buffer);
        var result = formatter.Deserialize(ref reader, options);
        reader.End.ShouldBeTrue();
        return result;
    }

    public static T RoundTrip<T>(IMessagePackFormatter<T> formatter, T value, MessagePackSerializerOptions options = null)
        => Decode(formatter, Encode(formatter, value, options).WrittenSpan.ToArray(), options);
}
