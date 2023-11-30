// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Newtonsoft.Json.Converters
{
    using global::Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using TypeInfo = Aqua.TypeSystem.TypeInfo;

    public abstract class ObjectConverter : JsonConverter
    {
        [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Preferred name")]
        protected sealed class Property
        {
            private readonly KnownTypesRegistry _knownTypes;

            public Property(PropertyInfo propertyInfo, KnownTypesRegistry knownTypes)
            {
                PropertyInfo = propertyInfo.CheckNotNull();
                _knownTypes = knownTypes.CheckNotNull();
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

        private static readonly Dictionary<Type, IReadOnlyCollection<Property>> _properties = new Dictionary<Type, IReadOnlyCollection<Property>>();

        protected ObjectConverter(KnownTypesRegistry knownTypes)
        {
            KnownTypesRegistry = knownTypes.CheckNotNull();
        }

        protected KnownTypesRegistry KnownTypesRegistry { get; }

        protected IReadOnlyCollection<Property> GetProperties(Type type)
        {
            type.AssertNotNull();
            lock (_properties)
            {
                if (!_properties.TryGetValue(type, out var propertySet))
                {
                    propertySet = type
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(x => x.CanRead && x.CanWrite)
                        .Where(x => x.GetIndexParameters().Length is 0)
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
    }
}
