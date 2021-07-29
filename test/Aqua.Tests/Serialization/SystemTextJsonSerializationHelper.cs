// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization
{
    using Aqua.Text.Json.Converters;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using System.Text.Json;
    using Xunit;

    public static class SystemTextJsonSerializationHelper
    {
        private static readonly JsonSerializerOptions _serializerSettings = new JsonSerializerOptions { WriteIndented = true }
            .AddConverter(new TimeSpanConverter())
            .ConfigureAqua();

        [SuppressMessage("Major Code Smell", "S125:Sections of code should not be commented out", Justification = "Debugging purpose")]
        public static T Serialize<T>(this T graph)
        {
            var json = JsonSerializer.Serialize(graph, _serializerSettings);

            // File.AppendAllText($"Dump-{graph?.GetType().Name}-JsonConvert-{Guid.NewGuid()}.json", json);
            return JsonSerializer.Deserialize<T>(json, _serializerSettings);
        }

        public static void SkipUnsupportedDataType(Type type, object value)
        {
            Skip.If(type.Is<BigInteger>(), $"{type} not supported by out-of-the-box System.Text.Json");
            Skip.If(type.Is<Complex>(), $"{type} not supported by out-of-the-box System.Text.Json");
#if !NET48
            Skip.If(type.Is<Half>(), $"{type} serialization is not supported.");
#endif // NET48
        }
    }
}