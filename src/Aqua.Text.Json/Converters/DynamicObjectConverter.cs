// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Text.Json.Converters;

using Aqua.Dynamic;
using Aqua.EnumerableExtensions;
using Aqua.TypeSystem;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using DynamicProperty = Aqua.Dynamic.Property;

public class DynamicObjectConverter : ObjectConverter<DynamicObject>
{
    private const string ValueProperty = "Value";
    private const string ValuesProperty = "Values";
    private const string ItemsProperty = "Items";

    public DynamicObjectConverter(KnownTypesRegistry knownTypes)
        : base(knownTypes)
    {
    }

    protected override void ReadObjectProperties(ref Utf8JsonReader reader, DynamicObject result, Dictionary<string, Property> properties, JsonSerializerOptions options)
    {
        reader.Advance();
        TypeInfo? typeInfo = null;
        void SetResult(ref Utf8JsonReader reader, IEnumerable<DynamicProperty>? properties = null, bool advance = true)
        {
            reader.AssertEndObject(advance);

            result.Type = typeInfo;
            if (properties?.Any() is true)
            {
                result.Properties = new PropertySet(properties);
            }
        }

        if (reader.IsProperty(nameof(DynamicObject.Type)))
        {
            typeInfo = reader.Read<TypeInfo?>(options);
            reader.Advance();
        }

        static bool IsProperty(ref Utf8JsonReader reader, string property, out bool isDynamicValueType)
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

        static JsonTokenType PeekTokenType(Utf8JsonReader r)
        {
            r.Advance();
            return r.TokenType;
        }

        if (IsProperty(ref reader, ValueProperty, out var isDynamicValue))
        {
            object? value;
            if (isDynamicValue)
            {
                value = reader.Read<DynamicObject>(options);
            }
            else if (PeekTokenType(reader) is JsonTokenType.String)
            {
                value = reader.Read(typeof(string), options);
            }
            else
            {
                value = reader.Read(typeInfo, options);
            }

            SetResult(ref reader, new[] { new DynamicProperty(string.Empty, value) });
            return;
        }

        if (IsProperty(ref reader, ItemsProperty, out isDynamicValue) || IsProperty(ref reader, ValuesProperty, out isDynamicValue))
        {
            reader.Advance();
            if (reader.TokenType is JsonTokenType.Null)
            {
                SetResult(ref reader);
                return;
            }

            var referenceResolver = options.ReferenceHandler?.CreateResolver();
            var reference = default(string);
            if (reader.TokenType is JsonTokenType.StartObject)
            {
                reader.Advance();
                if (reader.IsRefToken())
                {
                    if (referenceResolver is null)
                    {
                        throw reader.CreateException($"Cannot handle {JsonMetadata.RefToken} without reference resolver.");
                    }

                    var referenceId = reader.ReadString() ?? throw reader.CreateException($"{JsonMetadata.RefToken} must not be null");
                    reader.AssertEndObject();
                    var cachedArray = referenceResolver.ResolveReference(referenceId);
                    SetResult(ref reader, new[] { new DynamicProperty(string.Empty, cachedArray) });
                    return;
                }

                reader.AssertIdToken();

                reference = reader.ReadString();
                reader.Advance();

                reader.AssertValuesToken();
                reader.Advance();
            }

            if (reader.TokenType is not JsonTokenType.StartArray)
            {
                throw reader.CreateException($"Expected array");
            }

            var elementType = TypeHelper.GetElementType(typeInfo?.ToType()) ?? typeof(object);
            var itemType = isDynamicValue ? typeof(DynamicObject) : elementType;
            bool TryReadNextItem(ref Utf8JsonReader reader, out object? value)
            {
                var localItemType = PeekTokenType(reader) is JsonTokenType.String
                    ? typeof(string)
                    : itemType;
                if (!reader.TryRead(localItemType, options, out value))
                {
                    if (reader.TokenType is JsonTokenType.EndArray)
                    {
                        return false;
                    }

                    throw reader.CreateException("Unexpected token structure.");
                }

                return true;
            }

            var values = new List<object?>();
            while (TryReadNextItem(ref reader, out var item))
            {
                values.Add(item);
            }

            if (isDynamicValue)
            {
                elementType = typeof(object);
            }
            else if (values.Any(x => x is not null && (elementType == typeof(object) || !elementType.IsInstanceOfType(x))) &&
                values.All(static x => x is null || x is string))
            {
                elementType = typeof(string);
            }

            var valueArray = values.CastCollectionToArrayOfType(elementType);

            if (!string.IsNullOrWhiteSpace(reference))
            {
                referenceResolver?.AddReference(reference!, valueArray);

                reader.AssertEndObject();
            }

            SetResult(ref reader, new[] { new DynamicProperty(string.Empty, valueArray) });
            return;
        }

        if (reader.IsProperty(nameof(DynamicObject.Properties)))
        {
            reader.Advance();
            if (reader.TokenType is JsonTokenType.Null)
            {
                SetResult(ref reader);
                return;
            }

            if (reader.TokenType is not JsonTokenType.StartArray)
            {
                throw reader.CreateException("Expected array");
            }

            var propertySet = new List<DynamicProperty>();

            static bool NextItem(ref Utf8JsonReader reader)
            {
                reader.Advance();
                return reader.TokenType is not JsonTokenType.EndArray;
            }

            while (NextItem(ref reader))
            {
                reader.AssertStartObject(false);

                reader.AssertProperty(nameof(DynamicProperty.Name));
                var name = reader.ReadString() ?? throw reader.CreateException("Property name must not be null");

                reader.AssertProperty(nameof(Type));
                var type = reader.Read<TypeInfo?>(options);

                reader.AssertProperty(nameof(DynamicProperty.Value));
                var value = reader.Read(type, options);

                reader.AssertEndObject();
                propertySet.Add(new DynamicProperty(name, value));
            }

            SetResult(ref reader, propertySet);
            return;
        }

        if (reader.TokenType is JsonTokenType.EndObject)
        {
            SetResult(ref reader, advance: false);
            return;
        }

        throw reader.CreateException($"Unexpected token {reader.TokenType}");
    }

    protected override void WriteObjectProperties(Utf8JsonWriter writer, DynamicObject instance, IReadOnlyCollection<Property> properties, JsonSerializerOptions options)
    {
        var instanceType = instance.Type;
        var dynamicProperties = instance.Properties;
        if (TryGetWrappedValue(dynamicProperties, out var value))
        {
            var type = instanceType ?? CreateTypeInfo(value);

            writer.WritePropertyName(nameof(DynamicObject.Type));
            writer.Serialize(type, options);

            var propertyName = type.IsCollection() || value is object[]
                ? ItemsProperty
                : ValueProperty;
            var isDynamicValue =
                value is DynamicObject ||
                (value is object[] objectArray && objectArray.All(static x => x is null || x is DynamicObject) && objectArray.Any(static x => x is not null));
            if (isDynamicValue)
            {
                propertyName = $"Dynamic{propertyName}";
            }

            writer.WritePropertyName(propertyName);

            writer.Serialize(value, options);
        }
        else
        {
            if (instanceType is not null)
            {
                writer.WritePropertyName(nameof(DynamicObject.Type));
                writer.Serialize(instanceType, options);
            }

            if (dynamicProperties?.Count > 0)
            {
                writer.WritePropertyName(nameof(DynamicObject.Properties));

                writer.WriteStartArray();
                foreach (var property in dynamicProperties)
                {
                    writer.WriteStartObject();

                    writer.WriteString(nameof(property.Name), property.Name);

                    writer.WritePropertyName(nameof(Type));
                    writer.Serialize(CreateTypeInfo(property.Value), options);

                    writer.WritePropertyName(nameof(property.Value));
                    if (property.Value is null)
                    {
                        writer.WriteNullValue();
                    }
                    else
                    {
                        writer.Serialize(property.Value, property.Value.GetType(), options);
                    }

                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
            }
        }
    }

    private static bool TryGetWrappedValue(PropertySet? propertySet, [NotNullWhen(true)] out object? value)
    {
        if (propertySet?.Count is 1)
        {
            var p = propertySet.First();
            if (string.IsNullOrEmpty(p.Name) && p.Value is object v)
            {
                value = v;
                return true;
            }
        }

        value = null;
        return false;
    }

    [return: NotNullIfNotNull(nameof(value))]
    private static TypeInfo? CreateTypeInfo(object? value)
        => value is null
        ? null
        : new TypeInfo(value.GetType(), false, false);
}