// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Newtonsoft.Json.Converters;

using Aqua.TypeSystem;
using global::Newtonsoft.Json;

public static class JsonConverterHelper
{
    private static readonly Dictionary<Type, Func<JsonReader, object?>> _typeReaders = new (Type Type, Func<JsonReader, object?> Reader)[]
        {
            (typeof(bool), static r => r.ReadAsBoolean()),
            (typeof(byte[]), static r => r.ReadAsBytes()),
            (typeof(DateTime), static r => r.ReadAsDateTime()),
            (typeof(DateTimeOffset), static r => r.ReadAsDateTimeOffset()),
            (typeof(decimal), static r => r.ReadAsDecimal()),
            (typeof(double), static r => r.ReadAsDouble()),
            (typeof(int), static r => r.ReadAsInt32()),
            (typeof(string), static r => r.ReadAsString()),
        }
        .SelectMany(x => x.Type.IsClass
            ? new[] { x }
            : new[] { x, (Type: typeof(Nullable<>).MakeGenericType(x.Type), x.Reader) })
        .ToDictionary(static x => x.Type, static x => x.Reader);

    public static string IdToken => "$id";

    public static string RefToken => "$ref";

    public static string TypeToken => "$type";

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

    public static JsonSerializationException CreateException(this JsonReader reader, string message)
        => reader.CheckNotNull() is IJsonLineInfo lineInfo && lineInfo.HasLineInfo()
        ? new JsonSerializationException(message, reader.Path, lineInfo.LineNumber, lineInfo.LinePosition, null)
        : new JsonSerializationException(message);

    public static void Advance(this JsonReader reader, string? errorMessage = null)
    {
        reader.AssertNotNull();
        if (!reader.Read())
        {
            throw reader.CreateException(errorMessage ?? "Unexpected token structure.");
        }
    }

    public static void AssertProperty(this JsonReader reader, string propertyName, bool advance = true)
    {
        reader.AssertNotNull();
        if (advance)
        {
            reader.Advance();
        }

        if (!IsProperty(reader, propertyName))
        {
            throw reader.CreateException($"Expected token '{propertyName}'.");
        }
    }

    public static T? Read<T>(this JsonReader reader, JsonSerializer serializer)
        => reader.CheckNotNull().TryRead(serializer, out T? result)
        ? result
        : throw reader.CreateException("Unexpected token structure.");

    public static object? Read(this JsonReader reader, TypeInfo? type, JsonSerializer serializer)
        => reader.CheckNotNull().Read(type.MapTypeInfo(), serializer);

    public static object? Read(this JsonReader reader, Type? type, JsonSerializer serializer)
        => reader.CheckNotNull().TryRead(type, serializer, out object? result)
        ? result
        : throw reader.CreateException("Unexpected token structure.");

    public static bool TryRead<T>(this JsonReader reader, JsonSerializer serializer, out T? value)
    {
        var result = TryRead(reader, typeof(T), serializer, out var v);
        value = result && v is T x ? x : default;
        return result;
    }

    public static bool TryRead(this JsonReader reader, TypeInfo? type, JsonSerializer serializer, out object? result)
        => reader.TryRead(type.MapTypeInfo(), serializer, out result);

    public static bool TryRead(this JsonReader reader, Type? type, JsonSerializer serializer, out object? result)
    {
        reader.AssertNotNull();
        serializer.AssertNotNull();

        if (type is not null && _typeReaders.TryGetValue(type, out var read))
        {
            result = read(reader);
            return reader.TokenType is not JsonToken.EndArray
                && reader.TokenType is not JsonToken.EndObject;
        }

        reader.Advance($"Expected token object of type '{type}'.");

        switch (reader.TokenType)
        {
            case JsonToken.String:
                if (type != typeof(TypeInfo))
                {
                    var text = reader.Value as string;
                    result = type == typeof(char) && text?.Length > 0
                        ? text[0]
                        : text;
                    return true;
                }

                break;

            case JsonToken.Null:
                result = default;
                return true;

            case JsonToken.EndArray:
            case JsonToken.EndObject:
                result = default;
                return false;
        }

        result = serializer.Deserialize(reader, type);
        return true;
    }

    public static void AssertStartObject(this JsonReader reader, bool advance = true)
    {
        reader.AssertNotNull();
        if (advance && !reader.Read())
        {
            throw reader.CreateException($"Expected start object.");
        }

        if (reader.TokenType is not JsonToken.StartObject)
        {
            throw reader.CreateException($"Unexpected token type '{reader.TokenType}', expected {nameof(JsonToken.StartObject)} instead.");
        }
    }

    public static void AssertEndObject(this JsonReader reader, bool advance = true)
    {
        reader.AssertNotNull();
        if (advance)
        {
            reader.Advance();
        }

        if (reader.TokenType is not JsonToken.EndObject)
        {
            throw reader.CreateException($"Unexpected token type '{reader.TokenType}', expected {nameof(JsonToken.EndObject)} instead.");
        }
    }

    public static bool IsIdToken(this JsonReader reader) => IsProperty(reader, IdToken);

    public static bool IsRefToken(this JsonReader reader) => IsProperty(reader, RefToken);

    public static void AssertIdToken(this JsonReader reader, bool advance = false) => AssertProperty(reader, IdToken, advance);

    public static bool IsProperty(this JsonReader reader, string propertyName)
        => reader.CheckNotNull().TokenType is JsonToken.PropertyName
        && string.Equals(reader.Value as string, propertyName, StringComparison.Ordinal);

    public static bool TryWriteReference(this JsonWriter writer, JsonSerializer serializer, object value)
    {
        writer.AssertNotNull();
        var referenceResolver = serializer.CheckNotNull().ReferenceResolver;
        if (referenceResolver is null)
        {
            return false;
        }

        var exists = referenceResolver.IsReferenced(serializer, value);
        var reference = referenceResolver.GetReference(serializer, value);

        if (exists)
        {
            writer.WritePropertyName(RefToken);
        }
        else
        {
            writer.WritePropertyName(IdToken);
        }

        writer.WriteValue(reference);
        return exists;
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

    private static Type? MapTypeInfo(this TypeInfo? type)
    {
        var t = type?.ToType();
        return t == typeof(Type) ? typeof(TypeInfo) : t;
    }
}