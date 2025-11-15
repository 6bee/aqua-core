// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Text.Json.Converters;

using Aqua.TypeSystem;
using System;
using System.Text.Json;

public class TypeInfoConverter(KnownTypesRegistry knownTypes) : ObjectConverter<TypeInfo>(knownTypes)
{
    protected override TypeInfo? ReadJson(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.String &&
            reader.GetString() is string typeKey &&
            KnownTypesRegistry.TryGetTypeInfo(typeKey, out var typeInfo))
        {
            return typeInfo;
        }

        return base.ReadJson(ref reader, typeToConvert, options);
    }

    protected override void WriteJson(Utf8JsonWriter writer, TypeInfo value, JsonSerializerOptions options)
    {
        if (KnownTypesRegistry.TryGetTypeKey(value, out var typeKey))
        {
            writer.WriteStringValue(typeKey);
        }
        else
        {
            base.WriteJson(writer, value, options);
        }
    }
}