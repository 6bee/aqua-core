// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using Aqua.Extensions;
    using Aqua.Utils;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    public class TypeResolver : ITypeResolver
    {
        private static readonly ITypeResolver _defaultTypeResolver = new TypeResolver();

        private static ITypeResolver? _instance;

        private readonly TransparentCache<string, Type> _typeCache = new TransparentCache<string, Type>();

        private readonly Func<TypeInfo, Type> _typeEmitter;

        public TypeResolver(Func<TypeInfo, Type>? typeEmitter = null)
        {
            _typeEmitter = typeEmitter ?? new Emit.TypeEmitter(this).EmitType;
        }

        /// <summary>
        /// Gets or sets an instance of ITypeResolver.
        /// </summary>
        /// <remarks>
        /// Setting this property allows for registring a custom type resolver statically.
        /// Setting this property to null makes it fall-back to the default resolver.
        /// </remarks>
        [AllowNull]
        public static ITypeResolver Instance
        {
            get => _instance ?? _defaultTypeResolver;
            set => _instance = value;
        }

        [return: NotNullIfNotNull("typeInfo")]
        public virtual Type? ResolveType(TypeInfo? type)
        {
            if (type is null)
            {
                return null;
            }

            var cacheKey = Enumerable.Repeat(type.FullName, 1)
                .Concat(type.Properties?.Select(p => p.Name) ?? Enumerable.Empty<string>())
                .StringJoin(" ");

            var resolvedType = _typeCache.GetOrCreate(cacheKey, _ => ResolveTypeInternal(type));

            return ResolveOpenGenericType(type, resolvedType);
        }

        private Type ResolveTypeInternal(TypeInfo typeInfo)
        {
            Type? type = Type.GetType(typeInfo.FullName);
            if (!IsValid(typeInfo, type))
            {
                type = GetAssemblies()
                    .Select(x => x.GetType(typeInfo.FullName))
                    .FirstOrDefault(x => IsValid(typeInfo, x));
            }

            if (type is null)
            {
                try
                {
                    type = _typeEmitter(typeInfo);
                }
                catch (TypeResolverException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new TypeResolverException($"Type '{typeInfo.FullName}' could not be resolved and emitting type at runtime failed.", ex);
                }
            }

            if (type is null)
            {
                throw new TypeResolverException($"Type '{typeInfo.FullName}' could not be resolved, consider secifying custom {nameof(ITypeResolver)}.");
            }

            return type;
        }

        [return: NotNullIfNotNull("type")]
        private Type? ResolveOpenGenericType(TypeInfo typeInfo, Type? type)
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

        private static bool IsValid(TypeInfo typeInfo, Type? resolvedType)
        {
            if (resolvedType is null)
            {
                return false;
            }

            // validate properties if set in typeinfo
            if (typeInfo.Properties?.Any() == true)
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

        protected virtual IEnumerable<Assembly> GetAssemblies() => AppDomain.CurrentDomain.GetAssemblies();
    }
}
