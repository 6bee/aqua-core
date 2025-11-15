// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem;

using Aqua.EnumerableExtensions;
using Aqua.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

public class TypeResolver : ITypeResolver
{
    private static readonly TypeResolver _defaultTypeResolver = new();

    private static ITypeResolver? _instance;

    private readonly TransparentCache<string, Type> _typeCache = new();

    private readonly Lazy<Func<TypeInfo, Type>> _typeEmitter;

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeResolver"/> class.
    /// </summary>
    /// <param name="typeEmitter">Optional type emitter function. If this parameter is <see langword="null"/> and <see cref="AllowEmitType"/> is <see langword="true"/>, default type emitter is used.</param>
    public TypeResolver(Func<TypeInfo, Type>? typeEmitter = null)
        => _typeEmitter = new(() => typeEmitter ?? new Emit.TypeEmitter(this).EmitType);

    /// <summary>
    /// Gets a value indicating whether types which cannot be resoved are allowed to be emitted at runtime. Default value is <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// Emitting types requires <see cref="TypeInfo"/> to carry property information.
    /// </remarks>
    public bool AllowEmitType { get; init; } = true;

    /// <summary>
    /// Gets or sets an instance of ITypeResolver.
    /// </summary>
    /// <remarks>
    /// Setting this property allows for registring a custom type resolver statically.
    /// Setting this property to <see langword="null"/> makes it fall-back to the default resolver.
    /// </remarks>
    [AllowNull]
    public static ITypeResolver Instance
    {
        get => _instance ?? _defaultTypeResolver;
        set => _instance = value;
    }

    [return: NotNullIfNotNull(nameof(type))]
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
            var candidates = GetAssemblies()
                .Select(x => x.GetType(typeInfo.FullName)!)
                .Where(static x => x is not null)
                .Where(x => IsValid(typeInfo, x))
                .ToArray();

            if (candidates.Length > 1)
            {
                candidates = [.. candidates.OrderBy(x => x.IsPublic ? 0 : 1)];
            }

            type = candidates.FirstOrDefault();
        }

        if (type is null && AllowEmitType)
        {
            try
            {
                type = _typeEmitter.Value(typeInfo);
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

    [return: NotNullIfNotNull(nameof(type))]
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
                type = type.GetElementType()!.MakeGenericType(genericArguments!).MakeArrayType();
            }
            else
            {
                type = type.MakeGenericType(genericArguments!);
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
        if (typeInfo.Properties?.Count > 0)
        {
            var type = resolvedType.IsArray
                ? resolvedType.GetElementType()!
                : resolvedType;

            var resolvedProperties = type
                .GetProperties()
                .OrderBy(static x => x.MetadataToken)
                .Select(static x => x.Name);

            if (!typeInfo.Properties.Select(static x => x.Name).SequenceEqual(resolvedProperties))
            {
                return false;
            }
        }

        return true;
    }

    protected virtual IEnumerable<Assembly> GetAssemblies() => AppDomain.CurrentDomain.GetAssemblies();
}