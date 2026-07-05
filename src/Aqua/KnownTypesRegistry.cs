// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua;

using Aqua.Dynamic;
using Aqua.TypeExtensions;
using Aqua.TypeSystem;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

/// <summary>
/// Known types define the finit set of types allowed for deserialization.
/// </summary>
/// <remarks>
/// When serializing type information for known types, the type key (a short alias like "string", "uuid") substitutes the full type name during serialization.
/// </remarks>
public sealed class KnownTypesRegistry
{
    private static readonly IReadOnlyDictionary<Type, string> _typeAliases = new Dictionary<Type, string>
    {
        [typeof(string)] = "string",

        [typeof(byte[])] = "bytes",

        [typeof(byte)] = "uint8",
        [typeof(sbyte)] = "int8",

        [typeof(short)] = "int16",
        [typeof(ushort)] = "uint16",

        [typeof(int)] = "int32",
        [typeof(uint)] = "uint32",

        [typeof(long)] = "int64",
        [typeof(ulong)] = "uint64",

#if NET7_0_OR_GREATER
        [typeof(Int128)] = "int128",
        [typeof(UInt128)] = "uint128",
#endif // NET7_0_OR_GREATER

#if NET5_0_OR_GREATER
        [typeof(Half)] = "float16",
#endif // NET5_0_OR_GREATER
        [typeof(float)] = "float32",
        [typeof(double)] = "float64",
        [typeof(decimal)] = "decimal",

        [typeof(BigInteger)] = "bigint",
        [typeof(Complex)] = "complex128", // 2×64-bit floats
        [typeof(char)] = "char",
        [typeof(bool)] = "bool",
        [typeof(Guid)] = "uuid",

        [typeof(DateTime)] = "datetime",
        [typeof(TimeSpan)] = "timespan",
        [typeof(DateTimeOffset)] = "datetimeoffset",
#if NET6_0_OR_GREATER
        [typeof(DateOnly)] = "date",
        [typeof(TimeOnly)] = "time",
#endif // NET6_0_OR_GREATER

        [typeof(DynamicObject)] = "dynamic",
        [typeof(PropertySet)] = "propertyset",
        [typeof(Property)] = "property",

        // [typeof(MemberTypes)] = "membertype",
        [typeof(TypeInfo)] = "typeinfo",
        [typeof(ConstructorInfo)] = "constructorinfo",
        [typeof(FieldInfo)] = "fieldinfo",
        [typeof(MethodInfo)] = "methodinfo",
        [typeof(PropertyInfo)] = "propertyinfo",
    };

    private static readonly IReadOnlyCollection<(Type Type, string Key)> _defaultTypes = [..
        _typeAliases.Keys
        .SelectMany(static type =>
        {
            if (Nullable.GetUnderlyingType(type) is not null)
            {
                throw new InvalidEnumArgumentException($"Nullable types are created automatically and must not be specified in base list ({type.GetFriendlyName()})");
            }

            return new[]
            {
                type,
                type.IsValueType ? typeof(Nullable<>).MakeGenericType(type) : type,
                type.IsArray ? type : type.MakeArrayType(),
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
    /// <see langword="false"/> otherwise as either <paramref name="type"/> or <paramref name="typeKey"/> are already registered.</returns>
    public bool TryRegister(Type type, string? typeKey = null)
    {
        type.AssertNotNull();

        if (type.IsGenericTypeDefinition)
        {
            throw new ArgumentException($"Open generic types are not allowed: {type.GetFriendlyName()}");
        }

        typeKey ??= GetDefaultTypeName(type, true);

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

    private static string GetDefaultTypeName(Type type, bool useFullName = false)
    {
        if (_typeAliases.TryGetValue(type, out var alias))
        {
            return alias;
        }

        if (type.IsArray)
        {
            return $"{GetDefaultTypeName(type.GetElementType()!, useFullName)}[]";
        }

        if (Nullable.GetUnderlyingType(type) is { } underlying)
        {
            return $"{GetDefaultTypeName(underlying, useFullName)}?";
        }

        return type
            .GetFriendlyName(
                includeNamespance: useFullName,
                includeDeclaringType: useFullName)
            .ToLowerInvariant();
    }

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
        => new([], new(StringComparer.InvariantCultureIgnoreCase));
}
