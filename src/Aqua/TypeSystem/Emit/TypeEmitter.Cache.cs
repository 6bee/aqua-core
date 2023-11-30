// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem.Emit;

using Aqua.EnumerableExtensions;
using Aqua.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

partial class TypeEmitter
{
    private sealed class PropertyList
    {
        private readonly ReadOnlyCollection<string> _properties;
        private readonly Lazy<int> _hash;

        internal PropertyList(IEnumerable<string> properties)
        {
            _properties = properties.ToList().AsReadOnly();
            _hash = new Lazy<int>(_properties.GetCollectionHashCode);
        }

        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is PropertyList propertyList
                && Equals(propertyList);
        }

        private bool Equals(PropertyList other)
        {
            if (_properties.Count != other._properties.Count)
            {
                return false;
            }

            for (int i = 0; i < _properties.Count; i++)
            {
                if (!string.Equals(_properties[i], other._properties[i], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode() => _hash.Value;
    }

    private sealed class TypeWithPropertyList
    {
        private readonly ReadOnlyCollection<Tuple<string, Type>> _properties;
        private readonly Lazy<int> _hash;

        public TypeWithPropertyList(TypeInfo typeInfo, ITypeResolver typeResolver)
        {
            TypeFullName = typeInfo.FullName;

            var properties = typeInfo.Properties;
            _properties = properties is null
                ? new List<Tuple<string, Type>>().AsReadOnly()
                : properties.Select(x => CreatePropertyInfo(x, typeResolver)).ToList().AsReadOnly();

            _hash = new Lazy<int>(_properties.GetCollectionHashCode);
        }

        public string TypeFullName { get; }

        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is TypeWithPropertyList list
                && Equals(list);
        }

        private bool Equals(TypeWithPropertyList other)
        {
            if (!string.Equals(TypeFullName, other.TypeFullName, StringComparison.Ordinal))
            {
                return false;
            }

            if (_properties.Count != other._properties.Count)
            {
                return false;
            }

            return _properties.CollectionEquals(other._properties);
        }

        public override int GetHashCode() => _hash.Value;

        [return: NotNullIfNotNull(nameof(property))]
        private static Tuple<string, Type>? CreatePropertyInfo(PropertyInfo? property, ITypeResolver typeResolver)
        {
            if (property is null)
            {
                return null;
            }

            var propertyName = property.Name;
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("Property name missing");
            }

            var propertyTypeInfo = property.PropertyType;
            if (propertyTypeInfo is null)
            {
                throw new ArgumentException($"Property type missing for property '{propertyName}'");
            }

            var propertyType = propertyTypeInfo.ResolveType(typeResolver);

            return new Tuple<string, Type>(propertyName!, propertyType);
        }
    }

    private sealed class TypeCache : TransparentCache<object, Type>
    {
        private readonly ITypeResolver _typeResolver;

        public TypeCache(ITypeResolver typeResolver)
            => _typeResolver = typeResolver;

        internal Type GetOrCreate(IEnumerable<string> properties, Func<IEnumerable<string>, Type> factory)
        {
            var key = new PropertyList(properties);
            return GetOrCreate(key, x => factory(properties));
        }

        internal Type GetOrCreate(TypeInfo typeInfo, Func<TypeInfo, Type> factory)
        {
            var key = new TypeWithPropertyList(typeInfo, _typeResolver);
            return GetOrCreate(key, x => factory(typeInfo));
        }
    }
}