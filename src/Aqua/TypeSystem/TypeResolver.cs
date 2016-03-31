// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using Extensions;
    using System;
    using System.Linq;
    using System.Reflection;

    public partial class TypeResolver : ITypeResolver
    {
        private static readonly ITypeResolver _defaultTypeResolver = new TypeResolver();

        private static ITypeResolver _instance;

        private readonly TransparentCache<TypeInfo, Type> _typeCache = new TransparentCache<TypeInfo, Type>();

        private readonly TransparentCache<string, Type> _typeCacheByName = new TransparentCache<string, Type>();

        /// <summary>
        /// Sets or gets an instance of ITypeResolver.
        /// </summary>
        /// <remarks>
        /// Setting this property allows for registring a custom type resolver. 
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

        public virtual Type ResolveType(string typeName)
        {
            return _typeCacheByName.GetOrCreate(typeName, ResolveTypeInternal);
        }

        private Type ResolveTypeInternal(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentNullException("typeName", "Expected a valid type name");
            }

            var type = Type.GetType(typeName);
            if (ReferenceEquals(null, type))
            {
                var assemblies = GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    type = assembly.GetType(typeName);

                    if (!ReferenceEquals(null, type))
                    {
                        break;
                    }
                }
            }

            return type;
        }

        private Type ResolveTypeInternal(TypeInfo typeInfo)
        {
            var type = Type.GetType(typeInfo.FullName);
            if (!IsValid(type, typeInfo))
            {
                var assemblies = GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    type = assembly.GetType(typeInfo.FullName);

                    if (IsValid(type, typeInfo))
                    {
                        break;
                    }

                    type = null;
                }
            }

#if NET || NET35 || CORECLR
            if (ReferenceEquals(null, type))
            {
                type = _typeEmitter(typeInfo);
            }
#endif

            if (ReferenceEquals(null, type))
            {
                throw new Exception(string.Format("Type '{0}' could not be resolved", typeInfo.FullName));
            }

            if (typeInfo.IsGenericType)
            {
                var genericArguments = typeInfo.GenericArguments.Select(ResolveType).ToArray();

                if (typeInfo.IsArray)
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

        private static bool IsValid(Type type, TypeInfo typeInfo)
        {
            if (!ReferenceEquals(null, type))
            {
                if (typeInfo.IsArray)
                {
                    type = type.GetElementType();
                }

                if (typeInfo.IsAnonymousType || type.IsAnonymousType())
                {
                    var properties = type.GetProperties().Select(x => x.Name).ToList();
                    var propertyNames = typeInfo.Properties.Select(x => x.Name).ToList();

                    var match =
                        type.IsAnonymousType() &&
                        typeInfo.IsAnonymousType &&
                        properties.Count == propertyNames.Count &&
                        propertyNames.All(x => properties.Contains(x));

                    if (!match)
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
