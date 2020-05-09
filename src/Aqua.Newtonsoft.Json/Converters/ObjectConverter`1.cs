// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Newtonsoft.Json.Converters
{
    using global::Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    public class ObjectConverter<T> : ObjectConverter
        where T : class
    {
        public static string TypeToke => "$type";

        public Func<string, Type?>? DefaultTypeResolver { get; set; }

        public Func<Type, T>? DefaultObjectFactory { get; set; }

        public override bool CanConvert(Type objectType) => typeof(T).IsAssignableFrom(objectType);

        public override sealed object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            => ReadJson(reader, objectType, existingValue as T, serializer);

        public override sealed void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            => WriteJson(writer, value as T, serializer);

        public virtual T? ReadJson(JsonReader reader, Type objectType, T? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return default;
            }

            reader.AssertStartObject(false);

            reader.Advance();
            var referenceResolver = serializer.ReferenceResolver;
            if (reader.IsRefToken() && referenceResolver != null)
            {
                var referenceId = reader.ReadAsString() ?? throw reader.CreateException($"{JsonConverterHelper.RefToken} must not be null");
                reader.AssertEndObject();
                return (T)referenceResolver.ResolveReference(serializer, referenceId);
            }

            var reference = default(string);
            if (reader.IsIdToken())
            {
                reference = reader.ReadAsString();
                reader.Advance();
            }

            Type? type = null;
            if (reader.IsProperty(TypeToke))
            {
                var typeName = reader.ReadAsString();
                if (!string.IsNullOrEmpty(typeName))
                {
                    type = ResolveType(typeName!) ?? throw reader.CreateException($"Failed to resolve type '{typeName}'");
                }
            }

            if (type is null)
            {
                type = objectType;
            }

            var result = CreateObject(type) ?? throw reader.CreateException($"Failed create instance of type {type.FullName}");
            if (!string.IsNullOrWhiteSpace(reference) && referenceResolver != null)
            {
                referenceResolver.AddReference(serializer, reference!, result);
            }

            var properties = GetProperties(type);
            ReadObjectProperties(reader, result, properties.ToDictionary(x => x.Name), serializer);

            reader.AssertEndObject(false);

            return result;
        }

        public virtual void WriteJson(JsonWriter writer, T? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();

            if (!writer.TryWriteReference(serializer, value))
            {
                var type = value.GetType();

                writer.WritePropertyName(TypeToke);
                serializer.Serialize(writer, $"{type.FullName}, {type.Assembly.GetName().Name}");

                WriteObjectProperties(writer, value, GetProperties(type), serializer);
            }

            writer.WriteEndObject();
        }

        protected virtual void ReadObjectProperties(JsonReader reader, [DisallowNull] T result, Dictionary<string, Property> properties, JsonSerializer serializer)
        {
            while (reader.TokenType != JsonToken.EndObject)
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var name = (reader.Value as string) ?? throw reader.CreateException("Property name must not be null");
                    if (properties.TryGetValue(name, out var property))
                    {
                        var value = reader.Read(property.Type, serializer);
                        property.SetValue(result, value);
                    }
                    else
                    {
                        reader.Skip();
                    }
                }

                reader.Advance();
            }
        }

        protected virtual void WriteObjectProperties(JsonWriter writer, T instance, IReadOnlyCollection<Property> properties, JsonSerializer serializer)
        {
            foreach (var property in properties)
            {
                var value = property.GetValue(instance);
                if (property.EmitDefaultValue || !Equals(value, property.DefaultValue))
                {
                    writer.WritePropertyName(property.Name);
                    serializer.Serialize(writer, value, value?.GetType() ?? property.Type);
                }
            }
        }

        protected virtual Type? ResolveType(string typeName) => (DefaultTypeResolver ?? JsonConverterHelper.ResolveTypeName)(typeName);

        protected virtual T CreateObject(Type type) => (DefaultObjectFactory ?? FallbackObjectFactory)(type);

        private static T FallbackObjectFactory(Type type) => (T)Activator.CreateInstance(type);
    }
}
