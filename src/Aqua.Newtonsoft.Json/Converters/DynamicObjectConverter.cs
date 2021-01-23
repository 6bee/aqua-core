// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Newtonsoft.Json.Converters
{
    using Aqua.Dynamic;
    using Aqua.EnumerableExtensions;
    using Aqua.TypeSystem;
    using global::Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DynamicProperty = Aqua.Dynamic.Property;

    public class DynamicObjectConverter : ObjectConverter<DynamicObject>
    {
        private const string ValueProperty = "Value";
        private const string ValuesProperty = "Values";

        public DynamicObjectConverter(KnownTypesRegistry knownTypes)
            : base(knownTypes)
        {
        }

        protected override void ReadObjectProperties(JsonReader reader, DynamicObject result, Dictionary<string, Property> properties, JsonSerializer serializer)
        {
            reader.CheckNotNull(nameof(reader)).Advance();
            result.CheckNotNull(nameof(result));
            serializer.CheckNotNull(nameof(serializer));

            TypeInfo? typeInfo = null;
            void SetResult(IEnumerable<DynamicProperty>? properties = null)
            {
                reader.AssertEndObject();

                result.Type = typeInfo;
                if (properties?.Any() is true)
                {
                    result.Properties = new PropertySet(properties);
                }
            }

            if (reader.IsProperty(nameof(DynamicObject.Type)))
            {
                typeInfo = reader.Read<TypeInfo?>(serializer);
                reader.Advance();
            }

            bool IsProperty(string property, out bool isDynamicValueType)
            {
                isDynamicValueType = false;

                if (reader.IsProperty(property))
                {
                    return true;
                }

                if (reader.IsProperty($"Dynamic{property}"))
                {
                    isDynamicValueType = true;
                    return true;
                }

                return false;
            }

            if (IsProperty(ValueProperty, out var isDynamicValue))
            {
                var value = isDynamicValue
                    ? reader.Read<DynamicObject>(serializer)
                    : reader.Read(typeInfo, serializer);
                SetResult(new[] { new DynamicProperty(string.Empty, value) });
                return;
            }

            if (IsProperty(ValuesProperty, out isDynamicValue))
            {
                reader.Advance();
                if (reader.TokenType == JsonToken.Null)
                {
                    SetResult();
                    return;
                }

                if (reader.TokenType != JsonToken.StartArray)
                {
                    throw reader.CreateException($"Expected array");
                }

                var elementType = TypeHelper.GetElementType(typeInfo?.ToType()) ?? typeof(object);
                bool TryReadNextItem(out object? value)
                {
                    var itemType = isDynamicValue ? typeof(DynamicObject) : elementType;
                    if (!reader.TryRead(itemType, serializer, out value))
                    {
                        if (reader.TokenType == JsonToken.EndArray)
                        {
                            return false;
                        }

                        throw reader.CreateException("Unexpected token structure.");
                    }

                    return true;
                }

                var values = new List<object?>();
                while (TryReadNextItem(out var item))
                {
                    values.Add(item);
                }

                if (isDynamicValue)
                {
                    elementType = typeof(object);
                }
                else if (values.Any(x => x is not null && (elementType == typeof(object) || !elementType.IsInstanceOfType(x))) &&
                    values.All(x => x is null || x is string))
                {
                    elementType = typeof(string);
                }

                var valueArray = values.CastCollectionToArrayOfType(elementType);
                SetResult(new[] { new DynamicProperty(string.Empty, valueArray) });
                return;
            }

            if (reader.IsProperty(nameof(DynamicObject.Properties)))
            {
                reader.Advance();
                if (reader.TokenType == JsonToken.Null)
                {
                    SetResult();
                    return;
                }

                if (reader.TokenType != JsonToken.StartArray)
                {
                    throw reader.CreateException("Expected array");
                }

                var propertySet = new List<DynamicProperty>();

                bool NextItem()
                {
                    reader.Advance();
                    return reader.TokenType != JsonToken.EndArray;
                }

                while (NextItem())
                {
                    reader.AssertStartObject(false);

                    reader.AssertProperty(nameof(DynamicProperty.Name));
                    var name = reader.ReadAsString() ?? throw reader.CreateException("Property name must not be null");

                    reader.AssertProperty(nameof(Type));
                    var type = reader.Read<TypeInfo?>(serializer);

                    reader.AssertProperty(nameof(DynamicProperty.Value));
                    var value = reader.Read(type, serializer);

                    reader.AssertEndObject();
                    propertySet.Add(new DynamicProperty(name, value));
                }

                SetResult(propertySet);
                return;
            }

            throw reader.CreateException($"Unexpected token {reader.TokenType}");
        }

        protected override void WriteObjectProperties(JsonWriter writer, DynamicObject instance, IReadOnlyCollection<Property> properties, JsonSerializer serializer)
        {
            writer.CheckNotNull(nameof(writer));
            serializer.CheckNotNull(nameof(serializer));
            var instanceType = instance.CheckNotNull(nameof(instance)).Type;
            var dynamicProperties = instance.Properties;
            if (TryGetWrappedValue(dynamicProperties, out var value))
            {
                var type = instanceType ?? CreateTypeInfo(value);

                writer.WritePropertyName(nameof(DynamicObject.Type));
                serializer.Serialize(writer, type);

                var propertyName = type.IsCollection() ? ValuesProperty : ValueProperty;
                var isDynamicValue =
                    value is DynamicObject ||
                    (value is object[] objectArray && objectArray.Any(x => x is DynamicObject));
                if (isDynamicValue)
                {
                    propertyName = $"Dynamic{propertyName}";
                }

                writer.WritePropertyName(propertyName);
                serializer.Serialize(writer, value, type?.ToType());
            }
            else
            {
                if (instanceType is not null)
                {
                    writer.WritePropertyName(nameof(DynamicObject.Type));
                    serializer.Serialize(writer, instanceType);
                }

                if (dynamicProperties?.Any() is true)
                {
                    writer.WritePropertyName(nameof(DynamicObject.Properties));

                    writer.WriteStartArray();
                    foreach (var property in dynamicProperties)
                    {
                        writer.WriteStartObject();

                        writer.WritePropertyName(nameof(DynamicProperty.Name));
                        writer.WriteValue(property.Name);

                        writer.WritePropertyName(nameof(Type));
                        serializer.Serialize(writer, CreateTypeInfo(property.Value));

                        writer.WritePropertyName(nameof(DynamicProperty.Value));
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

        private static bool TryGetWrappedValue(PropertySet? propertySet, out object? value)
        {
            if (propertySet?.Count() == 1)
            {
                var p = propertySet.First();
                if (string.IsNullOrEmpty(p.Name) && p.Value is not null)
                {
                    value = p.Value;
                    return true;
                }
            }

            value = null;
            return false;
        }

        private static TypeInfo? CreateTypeInfo(object? value)
            => value is null
            ? null
            : new TypeInfo(value.GetType(), false, false);
    }
}
