// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;

    public static class DataContractSerializationHelper
    {
        public static T Serialize<T>(this T graph)
            => Serialize(graph, null);

        public static T Serialize<T>(this T graph, Type[] knownTypes)
        {
            var serializer = new DataContractSerializer(typeof(T), knownTypes);

            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, graph);
                stream.Dump($"Dump-{graph?.GetType().Name}-DataContractSerializer-{Guid.NewGuid()}.xml");
                stream.Seek(0, SeekOrigin.Begin);
                return (T)serializer.ReadObject(stream);
            }
        }
    }
}
