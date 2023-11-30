﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization;

using Aqua.Newtonsoft.Json;
using global::Newtonsoft.Json;

public static class NewtonsoftJsonSerializationHelper
{
    /// <summary>
    /// Gets pre-configured <see cref="JsonSerializerSettings"/> for <i>Aqua</i> types.
    /// </summary>
    public static JsonSerializerSettings SerializerSettings => new JsonSerializerSettings { Formatting = Formatting.Indented }.ConfigureAqua();

    public static T Clone<T>(this T graph)
    {
        var json = JsonConvert.SerializeObject(graph, SerializerSettings);
        return JsonConvert.DeserializeObject<T>(json, SerializerSettings);
    }
}