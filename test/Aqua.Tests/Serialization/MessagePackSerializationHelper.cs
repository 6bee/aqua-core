// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization;

using global::MessagePack;
using System.IO;

public static class MessagePackSerializationHelper
{
    public static T Clone<T>(this T graph)
    {
        var options = MessagePackSerializerOptions.Standard
            .ConfigureAqua()
            .WithPreserveReferences();

        using var stream = new MemoryStream();
        MessagePackSerializer.Serialize(stream, graph, options);

        stream.Position = 0;
        var copy = MessagePackSerializer.Deserialize<T>(stream, options);

        return copy;
    }
}
