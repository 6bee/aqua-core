// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization;

using Aqua.Text.Json.Converters;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

public static class SystemTextJsonSerializationHelper
{
    /// <summary>
    /// Gets pre-configured <see cref="JsonSerializerOptions"/> for <i>Aqua</i> types.
    /// </summary>
    public static JsonSerializerOptions SerializerOptions => new JsonSerializerOptions { WriteIndented = true, ReferenceHandler = ReferenceHandler.IgnoreCycles }
        .AddConverter(new TimeSpanConverter());

    public static T Clone<T>(this T graph)
        => Clone(graph, SerializerOptions);

    public static T Clone<T>(this T graph, JsonSerializerOptions options)
    {
        var json = JsonSerializer.Serialize(graph, options);
        return JsonSerializer.Deserialize<T>(json, options);
    }

    public static void SkipUnsupportedDataType(Type type, object value)
    {
        Skip.If(type.Is<BigInteger>(), $"{type} not supported by out-of-the-box System.Text.Json");
        Skip.If(type.Is<Complex>(), $"{type} not supported by out-of-the-box System.Text.Json");
    }
}