// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using Aqua.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public partial class TypeResolver : ITypeResolver
    {
        private sealed class EqualityComparer : IEqualityComparer<TypeInfo>, IEqualityComparer<PropertyInfo>
        {
            public static readonly EqualityComparer Instance = new EqualityComparer();

            private EqualityComparer()
            {
            }

            public bool Equals(TypeInfo x, TypeInfo y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (ReferenceEquals(null, x))
                {
                    return false;
                }

                if (ReferenceEquals(null, y))
                {
                    return false;
                }

                return string.Equals(x.ToString(), y.ToString())
                    && x.Properties.CollectionEquals(y.Properties, this);
            }

            public int GetHashCode(TypeInfo obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return 0;
                }

                return (obj.ToString().GetHashCode() * 397) ^ (obj.Properties?.Select(x => x.Name).GetCollectionHashCode() ?? 0);
            }

            public bool Equals(PropertyInfo x, PropertyInfo y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (ReferenceEquals(null, x))
                {
                    return false;
                }

                if (ReferenceEquals(null, y))
                {
                    return false;
                }

                return string.Equals(x.Name, y.Name)
                    && string.Equals(x.PropertyType?.ToString(), y.PropertyType?.ToString()); // break potential cyclic loop by comparing type name only
            }

            public int GetHashCode(PropertyInfo obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return 0;
                }
                
                return (obj.Name.GetHashCode() * 397) ^ (obj.PropertyType?.ToString().GetHashCode() ?? 0);
            }
        }

        private static readonly ITypeResolver _defaultTypeResolver = new TypeResolver();

        private static ITypeResolver _instance;

        private readonly TransparentCache<TypeInfo, Type> _typeCache = new TransparentCache<TypeInfo, Type>(comparer: EqualityComparer.Instance);

        /// <summary>
        /// Sets or gets an instance of ITypeResolver.
        /// </summary>
        /// <remarks>
        /// Setting this property allows for registring a custom type resolver statically. 
        /// Setting this property to null makes it fall-back to the default resolver.
        /// </remarks>
        public static ITypeResolver Instance
        {
            get { return _instance ?? _defaultTypeResolver; }
            set { _instance = value; }
        }

        public virtual Type ResolveType(TypeInfo typeInfo)
        {
            return _typeCache.GetOrCreate(typeInfo, ResolveTypeInternal);
        }

        private Type ResolveTypeInternal(TypeInfo typeInfo)
        {
            var type = Type.GetType(typeInfo.FullName);
            if (!IsValid(typeInfo, ref type))
            {
                var assemblies = GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    type = assembly.GetType(typeInfo.FullName);
                    if (IsValid(typeInfo, ref type))
                    {
                        break;
                    }

                    type = null;
                }
            }

#if NET || NETSTANDARD || CORECLR
            if (ReferenceEquals(null, type))
            {
                type = _typeEmitter(typeInfo);
                type = ResolveOpenGenericType(typeInfo, type);
            }
#endif

            if (ReferenceEquals(null, type))
            {
                throw new Exception($"Type '{typeInfo.FullName}' could not be resolved");
            }

            return type;
        }

        private Type ResolveOpenGenericType(TypeInfo typeInfo, Type type)
        {
            if (ReferenceEquals(null, type))
            {
                return null;
            }

            if (typeInfo.IsGenericType && !typeInfo.IsGenericTypeDefinition)
            {
                var genericArguments = (typeInfo.GenericArguments ?? Enumerable.Empty<TypeInfo>()).Select(ResolveType).ToArray();

                if (type.IsArray)
                {
                    type = type.GetElementType().MakeGenericType(genericArguments).MakeArrayType();
                }
                else
                {
                    type = type.MakeGenericType(genericArguments);
                }
            }

            return type;
        }

        private bool IsValid(TypeInfo typeInfo, ref Type resolvedType)
        {
            if (!ReferenceEquals(null, resolvedType))
            {
                resolvedType = ResolveOpenGenericType(typeInfo, resolvedType);

                if (typeInfo.Properties?.Any() ?? false) // can only validate properties if set in typeinfo
                {
                    var type = resolvedType.IsArray
                        ? resolvedType.GetElementType()
                        : resolvedType;

                    var resolvedProperties = type
                        .GetProperties()
                        .Select(x => new PropertyInfo { Name = x.Name, PropertyType = new TypeInfo(x.PropertyType, includePropertyInfos: false) });
                    
                    if (!typeInfo.Properties.CollectionEquals(resolvedProperties, EqualityComparer.Instance))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
    }
}
