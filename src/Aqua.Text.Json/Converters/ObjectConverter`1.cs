// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Text.Json.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class ObjectConverter<T> : JsonConverter<T>
        where T : class
    {
        [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Preferred name")]
        [DebuggerDisplay("{Name,nq}: {Type.Name,nq}")]
        protected sealed class Property
        {
            private readonly KnownTypesRegistry _knownTypes;

            public Property(PropertyInfo propertyInfo, KnownTypesRegistry knownTypes)
            {
                PropertyInfo = propertyInfo.CheckNotNull(nameof(propertyInfo));
                _knownTypes = knownTypes.CheckNotNull(nameof(knownTypes));
                IsIgnored = propertyInfo.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).Any();
                DataMemberAttribute = (DataMemberAttribute?)propertyInfo.GetCustomAttributes(typeof(DataMemberAttribute), false)?.FirstOrDefault();
                Name = string.IsNullOrWhiteSpace(DataMemberAttribute?.Name) ? propertyInfo.Name : DataMemberAttribute!.Name;
                EmitDefaultValue = DataMemberAttribute?.EmitDefaultValue is true;
                if (!EmitDefaultValue && propertyInfo.PropertyType.IsValueType)
                {
                    DefaultValue = Activator.CreateInstance(propertyInfo.PropertyType);
                }
            }

            private PropertyInfo PropertyInfo { get; }

            private DataMemberAttribute? DataMemberAttribute { get; }

            public string Name { get; }

            public Type Type => PropertyInfo.PropertyType;

            public bool EmitDefaultValue { get; }

            public object? DefaultValue { get; }

            public bool IsIgnored { get; }

            public int SortOrder => DataMemberAttribute?.Order ?? 0;

            public object? GetValue(object obj) => PropertyInfo.GetValue(obj);

            public void SetValue(object obj, object? value)
            {
                if (Type == typeof(TypeInfo) && value is string typeKey && _knownTypes.TryGetTypeInfo(typeKey, out var typeInfo))
                {
                    value = typeInfo;
                }

                PropertyInfo.SetValue(obj, value);
            }
        }

        [SuppressMessage("Major Code Smell", "S2743:Static fields should not be used in generic types", Justification = "Static field is specific for generic type")]
        private static readonly Dictionary<Type, IReadOnlyCollection<Property>> _properties
            = new Dictionary<Type, IReadOnlyCollection<Property>>();

        private readonly bool _handleSubtypes;

        public ObjectConverter(KnownTypesRegistry knownTypes)
            : this(knownTypes, false)
        {
        }

        public ObjectConverter(KnownTypesRegistry knownTypes, bool handleSubtypes)
        {
            knownTypes.AssertNotNull(nameof(knownTypes));
            KnownTypesRegistry = knownTypes;
            _handleSubtypes = handleSubtypes;
        }

        protected KnownTypesRegistry KnownTypesRegistry { get; }

        protected IReadOnlyCollection<Property> GetProperties(Type type)
        {
            type.AssertNotNull(nameof(type));
            lock (_properties)
            {
                if (!_properties.TryGetValue(type, out var propertySet))
                {
                    propertySet = type
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(x => x.CanRead && x.CanWrite)
                        .Where(x => x.GetIndexParameters().Length == 0)
                        .Select(x => new Property(x, KnownTypesRegistry))
                        .Where(x => !x.IsIgnored)
                        .OrderBy(x => x.SortOrder)
                        .ToList()
                        .AsReadOnly();
                    _properties.Add(type, propertySet);
                }

                return propertySet;
            }
        }

        public Func<string, Type?>? DefaultTypeResolver { get; set; }

        public Func<Type, T?>? DefaultObjectFactory { get; set; }

        public override bool CanConvert(Type typeToConvert)
            => _handleSubtypes
            ? typeof(T).IsAssignableFrom(typeToConvert)
            : typeToConvert == typeof(T);

        public sealed override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            typeToConvert.AssertNotNull(nameof(typeToConvert));
            options.AssertNotNull(nameof(options));

            return ReadJson(ref reader, typeToConvert, options.ToSessionOptions());
        }

        protected virtual T? ReadJson(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return default;
            }

            reader.AssertStartObject(false);

            reader.Advance();
            var referenceResolver = options.ReferenceHandler?.CreateResolver();
            if (reader.IsRefToken())
            {
                if (referenceResolver is null)
                {
                    throw reader.CreateException($"Cannot handle {JsonMetadata.RefToken} without reference resolver.");
                }

                var referenceId = reader.ReadString() ?? throw reader.CreateException($"{JsonMetadata.RefToken} must not be null");
                reader.AssertEndObject();
                return (T)referenceResolver.ResolveReference(referenceId);
            }

            var reference = default(string);
            if (reader.IsIdToken())
            {
                reference = reader.ReadString();
                reader.Advance();
            }

            Type? type = null;
            if (reader.IsProperty(JsonMetadata.TypeToken))
            {
                var typeName = reader.ReadString();
                if (typeName is not null && typeName.Length > 0)
                {
                    type = KnownTypesRegistry.TryGetTypeInfo(typeName, out var typeInfo)
                        ? typeInfo.ToType()
                        : ResolveType(typeName) ?? throw reader.CreateException($"Failed to resolve type '{typeName}'");
                }
            }

            if (type is null)
            {
                type = typeToConvert;
            }

            var result = CreateObject(type) ?? throw reader.CreateException($"Failed create instance of type {type.FullName}");
            if (!string.IsNullOrWhiteSpace(reference) && referenceResolver is not null)
            {
                referenceResolver.AddReference(reference!, result);
            }

            var properties = GetProperties(type);
            ReadObjectProperties(ref reader, result, properties.ToDictionary(x => x.Name), options);

            reader.AssertEndObject(false);

            return result;
        }

        public sealed override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.AssertNotNull(nameof(writer));
            options.AssertNotNull(nameof(options));

            WriteJson(writer, value, options.ToSessionOptions());
        }

        protected virtual void WriteJson(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();

            if (!writer.TryWriteReference(value, options))
            {
                var type = value.GetType();

                var typeName = KnownTypesRegistry.TryGetTypeKey(type, out var typeKey)
                    ? typeKey
                    : $"{type.FullName}, {type.Assembly.GetName().Name}";
                writer.WriteString(JsonMetadata.TypeToken, typeName);

                WriteObjectProperties(writer, value, GetProperties(type), options);
            }

            writer.WriteEndObject();
        }

        protected virtual void ReadObjectProperties(ref Utf8JsonReader reader, [DisallowNull] T result, Dictionary<string, Property> properties, JsonSerializerOptions options)
        {
            properties.AssertNotNull(nameof(properties));
            options.AssertNotNull(nameof(options));
            while (reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var name = reader.GetString() ?? throw reader.CreateException("Property name must not be null");
                    if (properties.TryGetValue(name, out var property))
                    {
                        var value = reader.Read(property.Type, options);
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

        protected virtual void WriteObjectProperties(Utf8JsonWriter writer, T instance, IReadOnlyCollection<Property> properties, JsonSerializerOptions options)
        {
            writer.AssertNotNull(nameof(writer));
            instance.AssertNotNull(nameof(instance));
            options.AssertNotNull(nameof(options));
            properties.AssertNotNull(nameof(properties));

            foreach (var property in properties)
            {
                var value = property.GetValue(instance);
                if (property.EmitDefaultValue || !Equals(value, property.DefaultValue))
                {
                    writer.WritePropertyName(property.Name);
                    writer.Serialize(value, value?.GetType() ?? property.Type, options);
                }
            }
        }

        protected virtual Type? ResolveType(string typeName) => (DefaultTypeResolver ?? JsonConverterHelper.ResolveTypeName)(typeName);

        protected virtual T? CreateObject(Type type) => (DefaultObjectFactory ?? FallbackObjectFactory)(type);

        private static T? FallbackObjectFactory(Type type) => (T?)Activator.CreateInstance(type);
    }
}