// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Newtonsoft.Json.Converters
{
    using global::Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;

    public abstract class ObjectConverter : JsonConverter
    {
        public static string TypeToke => "$type";

        protected sealed class Property
        {
            public Property(PropertyInfo propertyInfo)
            {
                PropertyInfo = propertyInfo ?? throw new ArgumentNullException(nameof(propertyInfo));
                IsIgnored = propertyInfo.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).Any();
                DataMemberAttribute = (DataMemberAttribute?)propertyInfo.GetCustomAttributes(typeof(DataMemberAttribute), false)?.FirstOrDefault();
                Name = string.IsNullOrWhiteSpace(DataMemberAttribute?.Name) ? propertyInfo.Name : DataMemberAttribute!.Name;
                EmitDefaultValue = DataMemberAttribute?.EmitDefaultValue == true;
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

            public void SetValue(object obj, object? value) => PropertyInfo.SetValue(obj, value);
        }

        private static readonly Dictionary<Type, IReadOnlyCollection<Property>> _properties = new Dictionary<Type, IReadOnlyCollection<Property>>();

        protected static IReadOnlyCollection<Property> GetProperties(Type type)
        {
            lock (_properties)
            {
                if (!_properties.TryGetValue(type, out var propertySet))
                {
                    propertySet = type
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(x => x.CanRead && x.CanWrite)
                        .Where(x => x.GetIndexParameters().Length == 0)
                        .Select(x => new Property(x))
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
