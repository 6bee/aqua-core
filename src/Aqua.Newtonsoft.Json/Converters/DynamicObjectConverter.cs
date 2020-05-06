// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Newtonsoft.Json.Converters
{
    using Aqua.Dynamic;
    using Aqua.TypeSystem;
    using global::Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class DynamicObjectConverter : JsonConverter<DynamicObject>
    {
        private const string IdToken = "$id";
        private const string RefToken = "$ref";

        public override DynamicObject ReadJson(JsonReader reader, Type objectType, DynamicObject existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            AssertStartObject(reader, false);

            Advance(reader);
            if (IsProperty(reader, RefToken))
            {
                var referenceId = reader.ReadAsString();
                AssertEndObject(reader);
                return (DynamicObject)serializer.ReferenceResolver.ResolveReference(serializer, referenceId);
            }

            if (!IsProperty(reader, IdToken))
            {
                throw new JsonSerializationException($"Expected token {IdToken}.");
            }

            var reference = reader.ReadAsString();
            Advance(reader);

            var result = new DynamicObject();
            serializer.ReferenceResolver.AddReference(serializer, reference, result);

            TypeInfo typeInfo = null;
            DynamicObject Result(IEnumerable<Property> properties = null)
            {
                result.Type = typeInfo;
                if (properties?.Any() == true)
                {
                    result.Properties = new PropertySet(properties);
                }

                return result;
            }

            if (IsProperty(reader, nameof(DynamicObject.Type)))
            {
                typeInfo = Read<TypeInfo>(reader, serializer);
                Advance(reader);
            }

            if (IsProperty(reader, "Value"))
            {
                var value = Read(typeInfo?.Type, reader, serializer);
                AssertEndObject(reader);
                return Result(new[] { new Property(string.Empty, value) });
            }
            else if (IsProperty(reader, "Values"))
            {
                Advance(reader);
                if (reader.TokenType == JsonToken.Null)
                {
                    AssertEndObject(reader);
                    return Result();
                }

                if (reader.TokenType != JsonToken.StartArray)
                {
                    throw new JsonSerializationException($"Expected array");
                }

                var values = new List<object>();
                while (true)
                {
                    var elementType = TypeHelper.GetElementType(typeInfo.Type);
                    var value = Read(elementType, reader, serializer);

                    // TODO: is max length quota required?
                    if (reader.TokenType == JsonToken.EndArray)
                    {
                        break;
                    }

                    values.Add(value);
                }

                AssertEndObject(reader);

                var elemetType = values.All(x => x is string) ? typeof(string) : TypeHelper.GetElementType(typeInfo.Type);
                object valueArray = DynamicObjectMapper.CastCollectionToArrayOfType(elemetType, values);
                return Result(new[] { new Property(string.Empty, valueArray) });
            }
            else if (IsProperty(reader, nameof(DynamicObject.Properties)))
            {
                Advance(reader);
                if (reader.TokenType == JsonToken.Null)
                {
                    AssertEndObject(reader);
                    return Result();
                }

                if (reader.TokenType != JsonToken.StartArray)
                {
                    throw new JsonSerializationException($"Expected array");
                }

                var propertySet = new List<Property>();
                while (true)
                {
                    Advance(reader);

                    // TODO: is max length quota required?
                    if (reader.TokenType == JsonToken.EndArray)
                    {
                        break;
                    }

                    AssertStartObject(reader, false);

                    AssertProperty(reader, nameof(Property.Name));
                    var name = reader.ReadAsString();

                    AssertProperty(reader, nameof(Type));
                    var type = Read<TypeInfo>(reader, serializer);

                    AssertProperty(reader, nameof(Property.Value));
                    var value = Read(type?.Type, reader, serializer);

                    AssertEndObject(reader);
                    propertySet.Add(new Property(name, value));
                }

                AssertEndObject(reader);

                return Result(propertySet);
            }
            else
            {
                throw new JsonSerializationException($"Unexpected token {reader.TokenType}");
            }
        }

        public override void WriteJson(JsonWriter writer, DynamicObject value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();

            if (!TryWriteReference(writer, serializer, value))
            {
                if (value.Properties?.Count() == 1 &&
                    string.IsNullOrEmpty(value.Properties.Single().Name) &&
                    value.Properties.Single().Value != null)
                {
                    var property = value.Properties.Single();
                    var type = value.Type ?? CreateTypeInfo(property);

                    writer.WritePropertyName(nameof(DynamicObject.Type));
                    serializer.Serialize(writer, type);

                    writer.WritePropertyName(IsCollection(type) ? "Values" : "Value");
                    serializer.Serialize(writer, property.Value, type?.Type);
                }
                else
                {
                    if (value.Type != null)
                    {
                        writer.WritePropertyName(nameof(DynamicObject.Type));
                        serializer.Serialize(writer, value.Type);
                    }

                    if (value.Properties?.Any() == true)
                    {
                        writer.WritePropertyName(nameof(DynamicObject.Properties));

                        writer.WriteStartArray();
                        foreach (var property in value.Properties)
                        {
                            writer.WriteStartObject();

                            writer.WritePropertyName(nameof(Property.Name));
                            writer.WriteValue(property.Name);

                            writer.WritePropertyName(nameof(Type));
                            serializer.Serialize(writer, CreateTypeInfo(property));

                            writer.WritePropertyName(nameof(Property.Value));
                            if (property.Value is null)
                            {
                                writer.WriteNull();
                            }
                            else
                            {
                                serializer.Serialize(writer, property.Value);
                            }

                            writer.WriteEndObject();
                        }

                        writer.WriteEndArray();
                    }
                }
            }

            writer.WriteEndObject();
        }

        private static bool IsCollection(TypeInfo type)
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

        private static void Advance(JsonReader reader, string errorMessage = null)
        {
            if (!reader.Read())
            {
                throw new JsonSerializationException(errorMessage ?? "Unexpected token structure.");
            }
        }

        private static void AssertProperty(JsonReader reader, string propertyName)
        {
            if (!reader.Read() || !IsProperty(reader, propertyName))
            {
                throw new JsonSerializationException($"Expected token '{propertyName}'.");
            }
        }

        private static T Read<T>(JsonReader reader, JsonSerializer serializer) => (T)Read(typeof(T), reader, serializer);

        private static object Read(Type type, JsonReader reader, JsonSerializer serializer)
        {
            if (type != null)
            {
                if (type == typeof(bool) || type == typeof(bool?))
                {
                    return reader.ReadAsBoolean();
                }

                if (type == typeof(byte[]))
                {
                    return reader.ReadAsBytes();
                }

                if (type == typeof(DateTime) || type == typeof(DateTime?))
                {
                    return reader.ReadAsDateTime();
                }

                if (type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?))
                {
                    return reader.ReadAsDateTimeOffset();
                }

                if (type == typeof(decimal) || type == typeof(decimal?))
                {
                    return reader.ReadAsDecimal();
                }

                if (type == typeof(double) || type == typeof(double?))
                {
                    return reader.ReadAsDouble();
                }

                if (type == typeof(int) || type == typeof(int?))
                {
                    return reader.ReadAsInt32();
                }

                if (type == typeof(string))
                {
                    return reader.ReadAsString();
                }
            }

            Advance(reader, $"Expected token object of type '{type}'.");

            switch (reader.TokenType)
            {
                case JsonToken.String:
                    return (string)reader.Value;

                case JsonToken.EndArray:
                case JsonToken.Null:
                case JsonToken.EndObject:
                    return default;
            }

            return serializer.Deserialize(reader, type);
        }

        private static void AssertStartObject(JsonReader reader, bool advance = true)
        {
            if (advance && !reader.Read())
            {
                throw new JsonSerializationException($"Expected start object.");
            }

            if (reader.TokenType != JsonToken.StartObject)
            {
                throw new JsonSerializationException($"Unexpected token type '{reader.TokenType}', expected {nameof(JsonToken.StartObject)} instead.");
            }
        }

        private static void AssertEndObject(JsonReader reader)
        {
            reader.Read();
            if (reader.TokenType != JsonToken.EndObject)
            {
                throw new JsonSerializationException($"Unexpected token type '{reader.TokenType}', expected {nameof(JsonToken.EndObject)} instead.");
            }
        }

        private static bool IsProperty(JsonReader reader, string propertyName)
            => reader.TokenType == JsonToken.PropertyName
            && string.Equals(reader.Value as string, propertyName, StringComparison.Ordinal);

        private bool TryWriteReference(JsonWriter writer, JsonSerializer serializer, object value)
        {
            var exists = serializer.ReferenceResolver.IsReferenced(serializer, value);
            var reference = serializer.ReferenceResolver.GetReference(serializer, value);

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

        private static TypeInfo CreateTypeInfo(Property property)
            => property.Value is null
            ? null
            : new TypeInfo(property.Value.GetType(), false, false);
    }
}
