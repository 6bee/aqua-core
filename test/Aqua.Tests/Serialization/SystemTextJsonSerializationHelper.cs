// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization
{
    using Aqua.Text.Json;
    using Aqua.Text.Json.Converters;
    using System;
    using System.Numerics;
    using System.Text.Json;
    using Xunit;

    public static class SystemTextJsonSerializationHelper
    {
        /// <summary>
        /// Gets pre-configured <see cref="JsonSerializerOptions"/> for <i>Aqua</i> types.
        /// </summary>
        public static JsonSerializerOptions SerializerOptions => new JsonSerializerOptions { WriteIndented = true }
            .AddConverter(new TimeSpanConverter())
            .ConfigureAqua();

        public static T Clone<T>(this T graph)
        {
            var json = JsonSerializer.Serialize(graph, SerializerOptions);
            return JsonSerializer.Deserialize<T>(json, SerializerOptions);
        }

        public static void SkipUnsupportedDataType(Type type, object value)
        {
            Skip.If(type.Is<BigInteger>(), $"{type} not supported by out-of-the-box System.Text.Json");
            Skip.If(type.Is<Complex>(), $"{type} not supported by out-of-the-box System.Text.Json");
#if NET5_0_OR_GREATER
            Skip.If(type.Is<Half>(), $"{type} serialization is not supported.");
#endif // NET5_0_OR_GREATER
        }
    }
}