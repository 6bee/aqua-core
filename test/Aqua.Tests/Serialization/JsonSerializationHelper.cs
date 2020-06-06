// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization
{
    using global::Newtonsoft.Json;
    using System.Diagnostics.CodeAnalysis;

    public static class JsonSerializationHelper
    {
        private static readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings { Formatting = Formatting.Indented }.ConfigureAqua();

        [SuppressMessage("Major Code Smell", "S125:Sections of code should not be commented out", Justification = "Debugging purpose")]
        public static T Serialize<T>(this T graph)
        {
            var json = JsonConvert.SerializeObject(graph, _serializerSettings);

            // File.AppendAllText($"Dump-{graph?.GetType().Name}-JsonConvert-{Guid.NewGuid()}.json", json);
            return JsonConvert.DeserializeObject<T>(json, _serializerSettings);
        }
    }
}
