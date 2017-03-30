// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if NET

namespace Aqua.Tests.Serialization
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;

    public static class NetDataContractSerializationHelper
    {
        public static T Serialize<T>(this T graph)
        {
            var serializer = new NetDataContractSerializer();

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, graph);
                stream.Dump($"Dump-{graph?.GetType().Name}-NetDataContractSerializer-{Guid.NewGuid()}.xml");
                stream.Seek(0, SeekOrigin.Begin);
                return (T)serializer.Deserialize(stream);
            }
        }
    }
}

#endif