// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Newtonsoft.Json.Converters
{
    using Aqua.TypeSystem;
    using global::Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    public static class JsonConverterHelper
    {
        private static readonly Dictionary<Type, Func<JsonReader, object?>> _typeReaders = new (Type Type, Func<JsonReader, object?> Reader)[]
            {
                (typeof(bool), r => r.ReadAsBoolean()),
                (typeof(byte[]), r => r.ReadAsBytes()),
                (typeof(DateTime), r => r.ReadAsDateTime()),
                (typeof(DateTimeOffset), r => r.ReadAsDateTimeOffset()),
                (typeof(decimal), r => r.ReadAsDecimal()),
                (typeof(double), r => r.ReadAsDouble()),
                (typeof(int), r => r.ReadAsInt32()),
                (typeof(string), r => r.ReadAsString()),
            }
            .SelectMany(x => x.Type.IsClass
                ? new[] { x }
                : new[] { x, (Type: typeof(Nullable<>).MakeGenericType(x.Type), x.Reader) })
            .ToDictionary(x => x.Type, x => x.Reader);

        public static string IdToken => "$id";

        public static string RefToken => "$ref";

        public static bool IsCollection(this TypeInfo? type)
        {
            if (type is null)
            {
                return false;
            }

            if (type.IsArray && type.Type != typeof(byte[]))
            {
                return true;
            }

            if (type.IsGenericType && type.Type.GetGenericTypeDefinition() == typeof(List<>))
            {
                return true;
            }

            return false;
        }

        public static JsonSerializationException CreateException(this JsonReader reader, string message)
            => reader is IJsonLineInfo lineInfo && lineInfo.HasLineInfo()
            ? new JsonSerializationException(message, reader.Path, lineInfo.LineNumber, lineInfo.LinePosition, null)
            : new JsonSerializationException(message);

        public static void Advance(this JsonReader reader, string? errorMessage = null)
        {
            if (!reader.Read())
            {
                throw reader.CreateException(errorMessage ?? "Unexpected token structure.");
            }
        }

        public static void AssertProperty(this JsonReader reader, string propertyName, bool advance = true)
        {
            if (advance)
            {
                reader.Advance();
            }

            if (!IsProperty(reader, propertyName))
            {
                throw reader.CreateException($"Expected token '{propertyName}'.");
            }
        }

        public static bool TryRead<T>(this JsonReader reader, JsonSerializer serializer, [MaybeNull] out T value)
        {
            var result = TryRead(reader, typeof(T), serializer, out var v);
            value = result && v is T x ? x : default;
            return result;
        }

        [return: MaybeNull]
        public static T Read<T>(this JsonReader reader, JsonSerializer serializer)
            => reader.TryRead(serializer, out T result)
            ? result
            : throw reader.CreateException("Unexpected token structure.");

        public static object? Read(this JsonReader reader, TypeInfo? type, JsonSerializer serializer)
            => reader.Read(type.MapTypeInfo(), serializer);

        public static object? Read(this JsonReader reader, Type? type, JsonSerializer serializer)
            => reader.TryRead(type, serializer, out object? result)
            ? result
            : throw reader.CreateException("Unexpected token structure.");

        public static bool TryRead(this JsonReader reader, TypeInfo type, JsonSerializer serializer, out object? result)
            => reader.TryRead(type.MapTypeInfo(), serializer, out result);

        public static bool TryRead(this JsonReader reader, Type? type, JsonSerializer serializer, out object? result)
        {
            if (type != null && _typeReaders.TryGetValue(type, out var read))
            {
                result = read(reader);
                return reader.TokenType != JsonToken.EndArray
                    && reader.TokenType != JsonToken.EndObject;
            }

            reader.Advance($"Expected token object of type '{type}'.");

            switch (reader.TokenType)
            {
                case JsonToken.String:
                    var text = reader.Value as string;
                    result = type == typeof(char) && text?.Length > 0
                        ? (object)text[0]
                        : text;
                    return true;

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
            if (advance && !reader.Read())
            {
                throw reader.CreateException($"Expected start object.");
            }

            if (reader.TokenType != JsonToken.StartObject)
            {
                throw reader.CreateException($"Unexpected token type '{reader.TokenType}', expected {nameof(JsonToken.StartObject)} instead.");
            }
        }

        public static void AssertEndObject(this JsonReader reader, bool advance = true)
        {
            if (advance)
            {
                reader.Advance();
            }

            if (reader.TokenType != JsonToken.EndObject)
            {
                throw reader.CreateException($"Unexpected token type '{reader.TokenType}', expected {nameof(JsonToken.EndObject)} instead.");
            }
        }

        public static bool IsIdToken(this JsonReader reader) => IsProperty(reader, IdToken);

        public static bool IsRefToken(this JsonReader reader) => IsProperty(reader, RefToken);

        public static void AssertIdToken(this JsonReader reader, bool advance = false) => AssertProperty(reader, IdToken, advance);

        public static bool IsProperty(this JsonReader reader, string propertyName)
            => reader.TokenType == JsonToken.PropertyName
            && string.Equals(reader.Value as string, propertyName, StringComparison.Ordinal);

        public static bool TryWriteReference(this JsonWriter writer, JsonSerializer serializer, object value)
        {
            var referenceResolver = serializer.ReferenceResolver;
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

        public static Type ResolveTypeName(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                throw new ArgumentException("Type name must not be emopty.", nameof(typeName));
            }

            return Type.GetType(typeName) ??
                AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(x => !x.IsDynamic)
                .Select(x => x.GetType(typeName))
                .Where(x => x != null)
                .FirstOrDefault();
        }

        private static Type? MapTypeInfo(this TypeInfo? type)
        {
            var t = type?.Type;
            return t == typeof(Type) ? typeof(TypeInfo) : t;
        }
    }
}
