// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization
{
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    public static class BinarySerializationHelper
    {
        public static T Serialize<T>(this T graph)
        {
            if (graph == null)
            {
                return graph;
            }

            var serializer = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, graph);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)serializer.Deserialize(stream);
            }
        }
    }
}
