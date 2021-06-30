// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization
{
    using MessagePack;
    using MessagePack.Resolvers;

    public static class MessagePackSerializationHelper
    {
        public static T Serialize<T>(this T graph)
        {
            var options = MessagePackSerializerOptions.Standard.WithResolver(TypelessObjectResolver.Instance);
            var bin = MessagePackSerializer.Serialize(graph, options);
            var copy = MessagePackSerializer.Deserialize<T>(bin, options);
            return copy;
        }
    }
}