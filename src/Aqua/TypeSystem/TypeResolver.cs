// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class TypeResolver : ITypeResolver
    {
        private static readonly ITypeResolver _defaultTypeResolver = new TypeResolver();

        private static ITypeResolver _instance;

        private readonly TransparentCache<string, Type> _typeCache = new TransparentCache<string, Type>();

        private readonly Func<TypeInfo, Type> _typeEmitter;

        public TypeResolver(Func<TypeInfo, Type> typeEmitter = null)
        {
            _typeEmitter = typeEmitter ?? new Emit.TypeEmitter().EmitType;
        }

        /// <summary>
        /// Gets or sets an instance of ITypeResolver.
        /// </summary>
        /// <remarks>
        /// Setting this property allows for registring a custom type resolver statically.
        /// Setting this property to null makes it fall-back to the default resolver.
        /// </remarks>
        public static ITypeResolver Instance
        {
            get => _instance ?? _defaultTypeResolver;
            set => _instance = value;
        }

        public virtual Type ResolveType(TypeInfo typeInfo)
        {
            var cacheKey = string.Join(
                " ",
                Enumerable.Repeat(typeInfo.FullName, 1).Concat(
                    typeInfo.Properties?.Select(p => p.Name)
                    ?? Enumerable.Empty<string>()));

            var type = _typeCache.GetOrCreate(cacheKey, _ => ResolveTypeInternal(typeInfo));

            return ResolveOpenGenericType(typeInfo, type);
        }

        private Type ResolveTypeInternal(TypeInfo typeInfo)
        {
            var type = Type.GetType(typeInfo.FullName);
            if (!IsValid(typeInfo, type))
            {
                var assemblies = GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    type = assembly.GetType(typeInfo.FullName);
                    if (IsValid(typeInfo, type))
                    {
                        break;
                    }

                    type = null;
                }
            }

            if (type is null)
            {
                type = _typeEmitter(typeInfo);
            }

            if (type is null)
            {
                throw new Exception($"Type '{typeInfo.FullName}' could not be resolved");
            }

            return type;
        }

        private Type ResolveOpenGenericType(TypeInfo typeInfo, Type type)
        {
            if (type is null)
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

        private bool IsValid(TypeInfo typeInfo, Type resolvedType)
        {
            if (!(resolvedType is null))
            {
                // can only validate properties if set in typeinfo
                if (typeInfo.Properties?.Any() ?? false)
                {
                    var type = resolvedType.IsArray
                        ? resolvedType.GetElementType()
                        : resolvedType;

                    var resolvedProperties = type
                        .GetProperties()
                        .OrderBy(x => x.MetadataToken)
                        .Select(x => x.Name);

                    if (!typeInfo.Properties.Select(x => x.Name).SequenceEqual(resolvedProperties))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        protected virtual IEnumerable<Assembly> GetAssemblies() => AppDomain.CurrentDomain.GetAssemblies();
    }
}
