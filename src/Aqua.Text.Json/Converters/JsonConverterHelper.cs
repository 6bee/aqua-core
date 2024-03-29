﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Text.Json.Converters;

using Aqua.TypeSystem;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class JsonConverterHelper
{
    public delegate object? Parse(ref Utf8JsonReader reader);

    private static readonly Dictionary<Type, Parse> _typeReaders = new (Type Type, Parse Reader)[]
        {
            (typeof(bool), static (ref Utf8JsonReader r) => r.GetBoolean()),
            (typeof(byte), static (ref Utf8JsonReader r) => r.GetByte()),
            (typeof(byte[]), static (ref Utf8JsonReader r) => r.GetBytesFromBase64()),
            (typeof(DateTime), (ref Utf8JsonReader r) => r.GetDateTime()),
            (typeof(DateTimeOffset), static (ref Utf8JsonReader r) => r.GetDateTimeOffset()),
            (typeof(decimal), static (ref Utf8JsonReader r) => r.GetDecimal()),
            (typeof(double), static (ref Utf8JsonReader r) => r.GetDouble()),
            (typeof(Guid), static (ref Utf8JsonReader r) => r.GetGuid()),
            (typeof(short), static (ref Utf8JsonReader r) => r.GetInt16()),
            (typeof(int), static (ref Utf8JsonReader r) => r.GetInt32()),
            (typeof(long), static (ref Utf8JsonReader r) => r.GetInt64()),
            (typeof(sbyte), static (ref Utf8JsonReader r) => r.GetSByte()),
            (typeof(float), static (ref Utf8JsonReader r) => r.GetSingle()),
            (typeof(string), static (ref Utf8JsonReader r) => r.GetString()),
            (typeof(ushort), static (ref Utf8JsonReader r) => r.GetUInt16()),
            (typeof(uint), static (ref Utf8JsonReader r) => r.GetUInt32()),
            (typeof(ulong), static (ref Utf8JsonReader r) => r.GetUInt64()),
        }
        .SelectMany(static x => x.Type.IsClass
            ? new[] { x }
            : new[] { x, (Type: typeof(Nullable<>).MakeGenericType(x.Type), x.Reader) })
        .ToDictionary(static x => x.Type, static x => x.Reader);

    public static bool IsCollection(this TypeInfo? type)
    {
        if (type is null)
        {
            return false;
        }

        if (type.IsArray && type.ToType() != typeof(byte[]))
        {
            return true;
        }

        if (type.IsGenericType && type.ToType().GetGenericTypeDefinition() == typeof(List<>))
        {
            return true;
        }

        return false;
    }

    public static JsonException CreateException(this ref Utf8JsonReader reader, string message)
        => new(message);

    public static void Advance(this ref Utf8JsonReader reader, string? errorMessage = null)
    {
        if (!reader.Read())
        {
            throw reader.CreateException(errorMessage ?? "Unexpected token structure.");
        }
    }

    public static void AssertProperty(this ref Utf8JsonReader reader, string propertyName, bool advance = true)
    {
        if (advance)
        {
            reader.Advance();
        }

        if (!IsProperty(ref reader, propertyName))
        {
            throw reader.CreateException($"Expected token '{propertyName}'.");
        }
    }

    public static T? Read<T>(this ref Utf8JsonReader reader, JsonSerializerOptions options)
        => reader.TryRead(options, out T? result)
        ? result
        : throw reader.CreateException("Unexpected token structure.");

    public static object? Read(this ref Utf8JsonReader reader, TypeInfo? type, JsonSerializerOptions options)
        => reader.Read(type.MapTypeInfo(), options);

    public static object? Read(this ref Utf8JsonReader reader, Type? type, JsonSerializerOptions options)
        => reader.TryRead(type, options, out object? result)
        ? result
        : throw reader.CreateException("Unexpected token structure.");

    public static bool TryRead<T>(this ref Utf8JsonReader reader, JsonSerializerOptions options, out T? value)
    {
        var result = reader.TryRead(typeof(T), options, out var v);
        value = result && v is T x ? x : default;
        return result;
    }

    public static bool TryRead(this ref Utf8JsonReader reader, TypeInfo? type, JsonSerializerOptions options, out object? result)
        => reader.TryRead(type.MapTypeInfo(), options, out result);

    public static bool TryRead(this ref Utf8JsonReader reader, Type? type, JsonSerializerOptions options, out object? result)
    {
        options.AssertNotNull();

        if (type is not null && _typeReaders.TryGetValue(type, out var read))
        {
            reader.Advance();

            if (reader.TokenType is JsonTokenType.Null)
            {
                result = null;
                return true;
            }

            var isValue =
                reader.TokenType is not JsonTokenType.EndArray &&
                reader.TokenType is not JsonTokenType.EndObject;

            result = isValue
                ? read(ref reader)
                : default;

            return isValue;
        }

        reader.Advance($"Expected token object of type '{type}'.");

        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                if (type != typeof(TypeInfo))
                {
                    var text = reader.GetString();
                    result = type == typeof(char) && text?.Length > 0
                        ? text[0]
                        : text;
                    return true;
                }

                break;

            case JsonTokenType.Null:
                result = default;
                return true;

            case JsonTokenType.EndArray:
            case JsonTokenType.EndObject:
                result = default;
                return false;
        }

        result = reader.Deserialize(type ?? typeof(object), options);
        return true;
    }

    public static void AssertStartObject(this ref Utf8JsonReader reader, bool advance = true)
    {
        if (advance && !reader.Read())
        {
            throw reader.CreateException($"Expected start object.");
        }

        if (reader.TokenType is not JsonTokenType.StartObject)
        {
            throw reader.CreateException($"Unexpected token type '{reader.TokenType}', expected {nameof(JsonTokenType.StartObject)} instead.");
        }
    }

    public static void AssertEndObject(this ref Utf8JsonReader reader, bool advance = true)
    {
        if (advance)
        {
            reader.Advance();
        }

        if (reader.TokenType is not JsonTokenType.EndObject)
        {
            throw reader.CreateException($"Unexpected token type '{reader.TokenType}', expected {nameof(JsonTokenType.EndObject)} instead.");
        }
    }

    public static bool IsIdToken(this ref Utf8JsonReader reader) => reader.IsProperty(JsonMetadata.IdToken);

    public static bool IsRefToken(this ref Utf8JsonReader reader) => reader.IsProperty(JsonMetadata.RefToken);

    public static void AssertIdToken(this ref Utf8JsonReader reader, bool advance = false) => reader.AssertProperty(JsonMetadata.IdToken, advance);

    public static void AssertValuesToken(this ref Utf8JsonReader reader, bool advance = false) => reader.AssertProperty(JsonMetadata.ValuesToken, advance);

    public static bool IsProperty(this ref Utf8JsonReader reader, string propertyName)
        => reader.TokenType is JsonTokenType.PropertyName
        && reader.ValueTextEquals(propertyName);

    public static bool TryWriteReference(this Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        writer.AssertNotNull();
        options.AssertNotNull();
        var referenceResolver = options.ReferenceHandler?.CreateResolver();
        if (referenceResolver is null)
        {
            return false;
        }

        var reference = referenceResolver.GetReference(value, out var alreadyExists);
        var propertyName = alreadyExists
            ? JsonMetadata.RefToken
            : JsonMetadata.IdToken;
        writer.WriteString(propertyName, reference);
        return alreadyExists;
    }

    public static Type? ResolveTypeName(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            throw new ArgumentException("Type name must not be emopty.", nameof(typeName));
        }

        return Type.GetType(typeName) ??
            AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(static x => !x.IsDynamic)
            .Select(x => x.GetType(typeName))
            .FirstOrDefault(static x => x is not null);
    }

    internal static Type? MapTypeInfo(this TypeInfo? type)
    {
        var t = type?.ToType();
        return t == typeof(Type) ? typeof(TypeInfo) : t;
    }

    public static string? ReadString(this ref Utf8JsonReader reader)
    {
        reader.Advance();
        return reader.GetString();
    }

    public static object? Deserialize(this ref Utf8JsonReader reader, Type returnType, JsonSerializerOptions options)
        => JsonSerializer.Deserialize(ref reader, returnType, options);

    public static void Serialize(this Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value, options);

    public static void Serialize(this Utf8JsonWriter writer, object? value, Type inputType, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value, inputType, options);
}