// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Newtonsoft.Json.Converters
{
    using global::Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;

    public class ObjectConverter<T> : JsonConverter<T>
    {
        public const string TypeToke = "$type";

        protected sealed class Property
        {
            public Property(PropertyInfo propertyInfo)
            {
                PropertyInfo = propertyInfo ?? throw new ArgumentNullException(nameof(propertyInfo));
                DataMemberAttribute = (DataMemberAttribute)propertyInfo.GetCustomAttributes(typeof(DataMemberAttribute), false)?.FirstOrDefault();
                Name = string.IsNullOrWhiteSpace(DataMemberAttribute?.Name) ? propertyInfo.Name : DataMemberAttribute.Name;
                EmitDefaultValue = DataMemberAttribute?.EmitDefaultValue == true;
                if (!EmitDefaultValue && propertyInfo.PropertyType.IsValueType)
                {
                    DefaultValue = Activator.CreateInstance(propertyInfo.PropertyType);
                }
            }

            private PropertyInfo PropertyInfo { get; }

            private DataMemberAttribute DataMemberAttribute { get; }

            public string Name { get; }

            public Type Type => PropertyInfo.PropertyType;

            public bool EmitDefaultValue { get; }

            public object DefaultValue { get; }

            public int SortOrder => DataMemberAttribute?.Order ?? 0;

            public object GetValue(object obj) => PropertyInfo.GetValue(obj);

            public void SetValue(object obj, object value) => PropertyInfo.SetValue(obj, value);
        }

        private static readonly Dictionary<Type, IReadOnlyCollection<Property>> _properties = new Dictionary<Type, IReadOnlyCollection<Property>>();

        private static IReadOnlyCollection<Property> GetProperties(Type type)
        {
            lock (_properties)
            {
                if (!_properties.TryGetValue(type, out IReadOnlyCollection<Property> property))
                {
                    property = type
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(x => x.CanRead && x.CanWrite)
                        .Where(x => x.GetIndexParameters().Length == 0)
                        .Select(x => new Property(x))
                        .OrderBy(x => x.SortOrder)
                        .ToList()
                        .AsReadOnly();
                    _properties.Add(type, property);
                }

                return property;
            }
        }

        public Func<string, Type> DefaultTypeResolver { get; set; }

        public Func<Type, T> DefaultObjectFactory { get; set; }

        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return default;
            }

            reader.AssertStartObject(false);

            reader.Advance();
            if (reader.IsRefToken())
            {
                var referenceId = reader.ReadAsString();
                reader.AssertEndObject();
                return (T)serializer.ReferenceResolver.ResolveReference(serializer, referenceId);
            }

            var reference = default(string);
            if (reader.IsIdToken())
            {
                reference = reader.ReadAsString();
                reader.Advance();
            }

            Type type;
            if (reader.IsProperty(TypeToke))
            {
                var typeName = reader.ReadAsString();
                type = ResolveType(typeName) ?? throw reader.CreateException($"Failed to resolve type '{typeName}'");
            }
            else
            {
                type = objectType;
            }

            var result = CreateObject(type);
            var properties = GetProperties(type);

            if (!string.IsNullOrWhiteSpace(reference))
            {
                serializer.ReferenceResolver.AddReference(serializer, reference, result);
            }

            ReadObjectProperties(reader, result, properties.ToDictionary(x => x.Name), serializer);

            reader.AssertEndObject(false);

            return result;
        }

        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
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
                serializer.Serialize(writer, type.FullName);

                WriteObjectProperties(writer, value, GetProperties(type), serializer);
            }

            writer.WriteEndObject();
        }

        protected virtual void ReadObjectProperties(JsonReader reader, T result, Dictionary<string, Property> properties, JsonSerializer serializer)
        {
            while (true)
            {
                reader.Advance();
                if (reader.TokenType == JsonToken.EndObject)
                {
                    break;
                }

                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var name = (string)reader.Value;
                    if (properties.TryGetValue(name, out Property property))
                    {
                        var value = reader.Read(property.Type, serializer);
                        property.SetValue(result, value);
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
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

        protected virtual Type ResolveType(string typeName) => (DefaultTypeResolver ?? JsonConverterHelper.ResolveTypeName)(typeName);

        protected virtual T CreateObject(Type type) => (DefaultObjectFactory ?? FallbackObjectFactory)(type);

        private static T FallbackObjectFactory(Type type) => (T)Activator.CreateInstance(type);
    }
}
