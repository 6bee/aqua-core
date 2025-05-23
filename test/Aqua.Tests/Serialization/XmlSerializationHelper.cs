﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization;

using System;
using System.IO;
using System.Numerics;
using System.Xml.Serialization;
using Xunit;

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

    public static void SkipUnsupportedDataType(Type type, object value)
    {
        Skip.If(type.Is<DateTimeOffset>(), $"{type} serialization is not supported");
        Skip.If(type.Is<TimeSpan>(), $"{type} serialization is not supported");
        Skip.If(type.Is<BigInteger>(), $"{type} serialization is not supported");
        Skip.If(type.Is<Complex>(), $"{type} serialization is not supported");
#if NET5_0_OR_GREATER
        Skip.If(type.Is<Half>(), $"{type} serialization is not supported.");
#endif // NET5_0_OR_GREATER
#if NET6_0_OR_GREATER
        Skip.If(type.Is<DateOnly>(), $"{type} serialization is not supported.");
        Skip.If(type.Is<TimeOnly>(), $"{type} serialization is not supported.");
#endif // NET6_0_OR_GREATER
#if NET7_0_OR_GREATER
        Skip.If(type.Is<Int128>(), $"{type} serialization is not supported.");
        Skip.If(type.Is<UInt128>(), $"{type} serialization is not supported.");
#endif // NET7_0_OR_GREATER
    }
}