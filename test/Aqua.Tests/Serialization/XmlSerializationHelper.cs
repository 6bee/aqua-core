﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization;

using System.IO;
using System.Xml.Serialization;

public static class XmlSerializationHelper
{
    public static T Serialize<T>(this T graph)
    {
        var serializer = new XmlSerializer(typeof(T));

        using var stream = new MemoryStream();
        serializer.Serialize(stream, graph);

        stream.Seek(0, SeekOrigin.Begin);
        return (T)serializer.Deserialize(stream);
    }
}
