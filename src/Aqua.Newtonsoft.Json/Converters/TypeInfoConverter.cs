// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Newtonsoft.Json.Converters;

using Aqua.TypeSystem;
using global::Newtonsoft.Json;
using System;

public class TypeInfoConverter : ObjectConverter<TypeInfo>
{
    public TypeInfoConverter(KnownTypesRegistry knownTypes)
        : base(knownTypes)
    {
    }

    public override TypeInfo? ReadJson(JsonReader reader, Type objectType, TypeInfo? existingValue, JsonSerializer serializer)
    {
        reader.AssertNotNull();
        serializer.AssertNotNull();

        if (reader.TokenType is JsonToken.String &&
            reader.Value is string typeKey &&
            KnownTypesRegistry.TryGetTypeInfo(typeKey, out var typeInfo))
        {
            return typeInfo;
        }

        return base.ReadJson(reader, objectType, existingValue, serializer);
    }

    public override void WriteJson(JsonWriter writer, TypeInfo? value, JsonSerializer serializer)
    {
        writer.AssertNotNull();

        if (value is not null && KnownTypesRegistry.TryGetTypeKey(value, out var typeKey))
        {
            writer.WriteValue(typeKey);
        }
        else
        {
            base.WriteJson(writer, value, serializer);
        }
    }
}