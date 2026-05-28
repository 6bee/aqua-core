// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Text.Json;

using Aqua.Dynamic;
using Aqua.TypeExtensions;
using Aqua.TypeSystem;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// When serializing type information for known types, the type key is written to json as a substitution of the former.
/// </summary>
public sealed class KnownTypesRegistry
{
    private static readonly IReadOnlyCollection<(Type Type, string Key)> _defaultTypes = [..
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
#if NET5_0_OR_GREATER
            typeof(Half),
#endif // NET5_0_OR_GREATER
#if NET6_0_OR_GREATER
            typeof(DateOnly),
            typeof(TimeOnly),
#endif // NET6_0_OR_GREATER
#if NET7_0_OR_GREATER
            typeof(Int128),
            typeof(UInt128),
#endif // NET7_0_OR_GREATER
        }
        .SelectMany(static type =>
        {
            if (Nullable.GetUnderlyingType(type) is not null)
            {
                throw new InvalidEnumArgumentException($"Nullable types are created automatically and must not be specified in base list ({type.GetFriendlyName()})");
            }

            if (type.IsArray)
            {
                throw new InvalidEnumArgumentException($"Array types are created automatically and must not be specified in base list ({type.GetFriendlyName()})");
            }

            return new[]
            {
                type,
                type.IsValueType ? typeof(Nullable<>).MakeGenericType(type) : type,
                type.MakeArrayType(),
            };
        })
        .Distinct()
        .Select(static type => (Type: type, Key: GetDefaultTypeName(type)))];

    private readonly Dictionary<Type, string> _keyLookup;
    private readonly Dictionary<string, TypeInfo> _typeLookup;

    /// <summary>
    /// Initializes a new instance of the <see cref="KnownTypesRegistry"/> class.
    /// </summary>
    private KnownTypesRegistry(Dictionary<Type, string> keyLookup, Dictionary<string, TypeInfo> typeLookup)
    {
        keyLookup.AssertNotNull();
        typeLookup.AssertNotNull();
        _keyLookup = keyLookup;
        _typeLookup = typeLookup;
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
    /// <returns><see langword="true"/> if type was successfully registered,
    /// <see langword="false"/> if either <paramref name="type"/> or <paramref name="typeKey"/> are already registered.</returns>
    public bool TryRegister(Type type, string? typeKey = null)
    {
        type.AssertNotNull();

        if (type.IsGenericTypeDefinition)
        {
            throw new ArgumentException($"Open generic types are not allowed: {type.GetFriendlyName()}");
        }

        typeKey ??= GetDefaultTypeName(type);

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

    private static string GetDefaultTypeName(Type type)
        => type switch
        {
            Type t when t == typeof(TypeInfo) => "type",
            Type t when t == typeof(DynamicObject) => "dynamic",
            Type t when t.Assembly == typeof(DynamicObject).Assembly => type.Name,
            Type t when t.IsArray => $"{GetDefaultTypeName(t.GetElementType()!)}[]",
            Type t when Nullable.GetUnderlyingType(t) is { } underlyingType => $"{GetDefaultTypeName(underlyingType)}?",
            _ => type.GetFriendlyName(includeNamespance: false, includeDeclaringType: false).ToLowerInvariant(),
        };

    /// <summary>
    /// Gets a new instance of the <see cref="KnownTypesRegistry"/> class with the default set of know types.
    /// </summary>
    public static KnownTypesRegistry Default
        => new(
            _defaultTypes.ToDictionary(static x => x.Type, static x => x.Key),
            _defaultTypes.ToDictionary(static x => x.Key, static x => CreateTypeInfo(x.Type), StringComparer.InvariantCultureIgnoreCase));

    /// <summary>
    /// Gets a new instance of the <see cref="KnownTypesRegistry"/> class.
    /// </summary>
    public static KnownTypesRegistry Empty
        => new([], []);
}