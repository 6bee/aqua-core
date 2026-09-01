// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization;

using global::MessagePack;

public static class MessagePackSerializationHelper
{
    public static T Clone<T>(this T graph)
    {
        var options = MessagePackSerializerOptions.Standard
            .ConfigureAqua()
            .WithPreserveReferences();
        var data = MessagePackSerializer.Serialize(graph, options);
        var copy = MessagePackSerializer.Deserialize<T>(data, options);
        return copy;
    }
}
