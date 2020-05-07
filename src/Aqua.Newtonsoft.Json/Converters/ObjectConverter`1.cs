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
        public const string ValueToke = "$value";

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

            reader.AssertIdToken();
            var reference = reader.ReadAsString();

            reader.AssertProperty(TypeToke);
            var typeName = reader.ReadAsString();
            var type = ResolveType(typeName);

            reader.AssertProperty(ValueToke);
            var properties = GetProperties(type);

            var result = CreateObject(type);
            serializer.ReferenceResolver.AddReference(serializer, reference, result);

            reader.AssertStartObject();
            ReadObject(reader, result, properties.ToDictionary(x => x.Name), serializer);
            reader.AssertEndObject();

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

                writer.WritePropertyName(ValueToke);
                WriteObject(writer, value, GetProperties(type), serializer);
            }

            writer.WriteEndObject();
        }

        protected virtual void ReadObject(JsonReader reader, T result, Dictionary<string, Property> properties, JsonSerializer serializer)
        {
            reader.AssertStartObject(false);
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

            reader.AssertEndObject(false);
        }

        protected virtual void WriteObject(JsonWriter writer, T instance, IReadOnlyCollection<Property> properties, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            foreach (var property in properties)
            {
                var value = property.GetValue(instance);
                if (property.EmitDefaultValue || !Equals(value, property.DefaultValue))
                {
                    writer.WritePropertyName(property.Name);
                    serializer.Serialize(writer, value, value?.GetType() ?? property.Type);
                }
            }

            writer.WriteEndObject();
        }

        protected virtual Type ResolveType(string typeName) => (DefaultTypeResolver ?? JsonConverterHelper.ResolveTypeName)(typeName);

        protected virtual T CreateObject(Type type) => (DefaultObjectFactory ?? FallbackObjectFactory)(type);

        private static T FallbackObjectFactory(Type type) => (T)Activator.CreateInstance(type);
    }
}
