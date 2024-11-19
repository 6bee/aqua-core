// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Newtonsoft.Json;

using Aqua.Dynamic;
using Aqua.TypeSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

/// <summary>
/// When serializing type information for known types, the type key is written to json as a substitution of the former.
/// </summary>
public sealed class KnownTypesRegistry
{
    private static readonly IReadOnlyCollection<(Type Type, string Key)> _defaultTypes =
        new[]
        {
            typeof(string),
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(char),
            typeof(bool),
            typeof(Guid),
            typeof(DateTime),
            typeof(TimeSpan),
            typeof(DateTimeOffset),
#if NET8_0_OR_GREATER
            typeof(Half),
#endif // NET8_0_OR_GREATER
            typeof(DynamicObject),
            typeof(Property),
            typeof(PropertySet),
            typeof(ConstructorInfo),
            typeof(FieldInfo),
            typeof(MemberInfo),
            typeof(MemberTypes),
            typeof(MethodBaseInfo),
            typeof(MethodInfo),
            typeof(PropertyInfo),
            typeof(TypeInfo),
        }
        .Select(static x =>
        {
            var name = x switch
            {
                Type t when t == typeof(TypeInfo) => "type",
                Type t when t == typeof(DynamicObject) => "dynamic",
                Type t when t.Assembly == typeof(DynamicObject).Assembly => x.Name,
                _ => x.Name.ToLowerInvariant(),
            };

            return (Type: x, Key: name);
        })
        .SelectMany(static x => new[]
            {
                x,
                x.Type.IsValueType ? (typeof(Nullable<>).MakeGenericType(x.Type), $"{x.Key}?") : x,
                (x.Type.MakeArrayType(), $"{x.Key}[]"),
            })
        .Distinct()
        .ToArray();

    private readonly Dictionary<Type, string> _keyLookup;
    private readonly Dictionary<string, TypeInfo> _typeLookup;

    public KnownTypesRegistry()
    {
        _keyLookup = _defaultTypes.ToDictionary(static x => x.Type, static x => x.Key);
        _typeLookup = _defaultTypes.ToDictionary(static x => x.Key, static x => CreateTypeInfo(x.Type), StringComparer.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Register specified <see cref="Type"/> as known type, unless <typeparamref name="T"/> or <paramref name="typeKey"/> have already been registered.
    /// </summary>
    /// <returns><see langword="true"/> is type was successfully registered,
    /// <see langword="false"/> if either <typeparamref name="T"/> or <paramref name="typeKey"/> are already registered.</returns>
    public bool TryRegister<T>(string? typeKey = null) => TryRegister(typeof(T), typeKey);

    /// <summary>
    /// Register specified <see cref="Type"/> as known type, unless <paramref name="type"/> or <paramref name="typeKey"/> have already been registered.
    /// </summary>
    /// <returns><see langword="true"/> is type was successfully registered,
    /// <see langword="false"/> if either <paramref name="type"/> or <paramref name="typeKey"/> are already registered.</returns>
    public bool TryRegister(Type type, string? typeKey = null)
    {
        type.AssertNotNull();

        typeKey ??= type.Name.ToLowerInvariant();

        lock (_keyLookup)
        {
            if (_keyLookup.ContainsKey(type) || _typeLookup.ContainsKey(typeKey))
            {
                return false;
            }

            _keyLookup.Add(type, typeKey);
            _typeLookup.Add(typeKey, CreateTypeInfo(type));
            return true;
        }
    }

    public bool TryGetTypeInfo(string key, [MaybeNullWhen(false)] out TypeInfo typeInfo)
    {
        typeInfo = _typeLookup.TryGetValue(key, out var type)
            ? new TypeInfo(type)
            : null;
        return typeInfo is not null;
    }

    public bool TryGetTypeKey(TypeInfo type, [MaybeNullWhen(false)] out string typeKey) => TryGetTypeKey(type.ToType(), out typeKey);

    public bool TryGetTypeKey(Type type, [MaybeNullWhen(false)] out string typeKey) => _keyLookup.TryGetValue(type, out typeKey);

    private static TypeInfo CreateTypeInfo(Type type) => new(type, false, false);
}