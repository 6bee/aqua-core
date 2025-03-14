﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Dynamic;

using Aqua.EnumerableExtensions;
using Aqua.TypeExtensions;
using Aqua.TypeSystem;
using Aqua.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using static Aqua.Dynamic.UnmappedAttributeHelper;
using ConstructorInfo = System.Reflection.ConstructorInfo;
using FieldInfo = System.Reflection.FieldInfo;
using MethodInfo = System.Reflection.MethodInfo;
using PropertyInfo = System.Reflection.PropertyInfo;
using TypeInfo = System.Reflection.TypeInfo;

public partial class DynamicObjectMapper : IDynamicObjectMapper
{
    private static class Helper
    {
#pragma warning disable S2094 // Classes should not be empty
        /// <summary>
        /// Type definition used in generic type filters.
        /// </summary>
        private sealed class TElement
        {
        }
#pragma warning restore S2094 // Classes should not be empty

        public static readonly MethodInfo AddAllMethodInfo = typeof(Helper).GetMethodEx(
            nameof(AddAll),
            [typeof(TElement)],
            typeof(IEnumerable<TElement>),
            typeof(object),
            typeof(MethodInfo));

        public static void AddAll<TElement>(IEnumerable<TElement> source, object target, MethodInfo addMethod)
            => source.ForEach(x => addMethod.Invoke(target, [(object?)x]));
    }

    private sealed class InternalDynamicObjectFactory : IDynamicObjectFactory
    {
        private readonly ITypeInfoProvider _typeInfoProvider;

        public InternalDynamicObjectFactory(ITypeInfoProvider? typeInfoProvider)
        {
            _typeInfoProvider = typeInfoProvider ?? new TypeInfoProvider();
        }

        public DynamicObject CreateDynamicObject(Type? type, object instance)
        {
            var typeInfo = type is null ? null : _typeInfoProvider.GetTypeInfo(type);
            return new DynamicObject(typeInfo);
        }
    }

    [DebuggerDisplay("{Type} {Value}")]
    private readonly struct ReferenceMapKey : IEquatable<ReferenceMapKey>
    {
        public ReferenceMapKey(Type type, DynamicObject value)
        {
            Type = type;
            Value = value;
        }

        public Type Type { get; }

        public DynamicObject Value { get; }

        public override bool Equals(object? obj)
            => obj is ReferenceMapKey other
            && Equals(other);

        public bool Equals(ReferenceMapKey other)
            => EqualityComparer<Type>.Default.Equals(Type, other.Type)
            && ReferenceEqualityComparer<DynamicObject>.Default.Equals(Value, other.Value);

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Type?.GetHashCode() ?? 0) * 397) ^ (Value?.GetHashCode() ?? 0);
            }
        }
    }

    private interface IMappingContext
    {
        void Recycle();
    }

    /// <summary>
    /// Execution context used for mapping to <see cref="DynamicObject"/>s.
    /// </summary>
    private sealed class ToContext : IMappingContext
    {
        private readonly Func<Type, Type> _dynamicObjectTypeInfoMapper;
        private readonly Dictionary<object, DynamicObject> _referenceMap;

        public ToContext(ITypeMapper? typeMapper)
        {
            _dynamicObjectTypeInfoMapper = typeMapper is null ? (static t => t) : typeMapper.MapType;
            _referenceMap = new Dictionary<object, DynamicObject>(ReferenceEqualityComparer<object>.Default);
        }

        /// <summary>
        /// Returns an existing instance if found in the reference map, creates a new instance otherwise.
        /// </summary>
        internal DynamicObject TryGetOrCreateNew(Type sourceType, object from, Func<Type?, object, Func<Type, bool>, DynamicObject> factory, Action<Type, object, DynamicObject, Func<Type, bool>>? initializer, Func<Type, bool> setTypeInformation)
        {
            if (!_referenceMap.TryGetValue(from, out var to))
            {
                var setTypeInformationValue = setTypeInformation(sourceType);

                to = factory(setTypeInformationValue ? _dynamicObjectTypeInfoMapper(sourceType) : null, from, setTypeInformation);

                try
                {
                    _referenceMap.Add(from, to);
                }
                catch
                {
                    // detected cyclic reference
                    // can happen for non-serializable types without parameterless constructor, which have cyclic references
                    return _referenceMap[from];
                }

                initializer?.Invoke(sourceType, from, to, setTypeInformation);
            }

            return to;
        }

        public void Recycle() => _referenceMap.Clear();
    }

    /// <summary>
    /// Execution context used for mapping from <see cref="DynamicObject"/>s.
    /// </summary>
    private sealed class FromContext : IMappingContext
    {
        private readonly Dictionary<ReferenceMapKey, object> _referenceMap;
        private readonly HashSet<Type> _safeTypes;
        private readonly ITypeSafetyChecker? _typeSafetyChecker;

        public FromContext(ITypeSafetyChecker? typeSafetyChecker)
        {
            _referenceMap = [];
            _safeTypes = [];
            _typeSafetyChecker = typeSafetyChecker;
        }

        /// <summary>
        /// Returns an existing instance if found in the reference map, creates a new instance otherwise.
        /// </summary>
        internal object TryGetOrCreateNew(Type targetType, DynamicObject from, Func<Type, DynamicObject, object> factory, Action<Type, DynamicObject, object>? initializer)
        {
            var key = new ReferenceMapKey(targetType, from);
            if (!_referenceMap.TryGetValue(key, out var to))
            {
                if (_typeSafetyChecker is not null && !_safeTypes.Contains(targetType))
                {
                    _typeSafetyChecker.AssertTypeSafety(targetType);
                    _safeTypes.Add(targetType);
                }

                to = factory(targetType, from);

                try
                {
                    _referenceMap.Add(key, to);
                }
                catch
                {
                    // detected cyclic reference
                    // can happen for non-serializable types without parameterless constructor, which have cyclic references
                    return _referenceMap[key];
                }

                initializer?.Invoke(targetType, from, to);
            }

            return to;
        }

        public void Recycle() => _referenceMap.Clear();
    }

    private const string NumericPattern = @"([0-9]*\.?[0-9]+|[0-9]+\.?[0-9]*)([eE][+-]?[0-9]+)?";

    private static readonly string ComplexNumberParserRegexPattern = $"^(?<Re>[+-]?({NumericPattern}))(?<Sign>[+-])[iI](?<Im>{NumericPattern})$";

    private static readonly Type _genericDictionaryType = typeof(Dictionary<,>);

    private static readonly Type _genericKeyValuePairType = typeof(KeyValuePair<,>);

    private static readonly Func<Type, bool> _isNativeType = new[]
        {
            typeof(string),
            typeof(int),
            typeof(uint),
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
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
            typeof(System.Numerics.BigInteger),
            typeof(System.Numerics.Complex),
            typeof(byte[]),
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
        .SelectMany<Type, Type>(static x => x.IsValueType ? [x, typeof(Nullable<>).MakeGenericType(x)] : [x])
        .ToHashSet()
        .Contains;

    private static readonly Dictionary<Type, HashSet<Type>> _implicitNumericConversionsTable =
        new Dictionary<Type, Type[]>
        {
            // source: http://msdn.microsoft.com/en-us/library/y5b434w4.aspx
            { typeof(sbyte), new[] { typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) } },
            { typeof(byte), new[] { typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) } },
            { typeof(short), new[] { typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) } },
            { typeof(ushort), new[] { typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) } },
            { typeof(int), new[] { typeof(long), typeof(float), typeof(double), typeof(decimal) } },
            { typeof(uint), new[] { typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) } },
            { typeof(long), new[] { typeof(float), typeof(double), typeof(decimal) } },
            { typeof(char), new[] { typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) } },
            { typeof(float), new[] { typeof(double) } },
            { typeof(ulong), new[] { typeof(float), typeof(double), typeof(decimal) } },
#if NET7_0_OR_GREATER
            // source: https://learn.microsoft.com/en-us/dotnet/api/system.int128.op_implicit
            { typeof(Int128), new[] { typeof(sbyte), typeof(ulong), typeof(uint), typeof(ushort), typeof(int), typeof(short), typeof(long), typeof(char), typeof(byte) } },

            // source: https://learn.microsoft.com/en-us/dotnet/api/system.uint128.op_implicit
            { typeof(UInt128), new[] { typeof(byte), typeof(char), typeof(ushort), typeof(uint), typeof(ulong) } },
#endif // NET7_0_OR_GREATER
        }
        .ToDictionary(static x => x.Key, static x => x.Value.ToHashSet());

    private static readonly Dictionary<Type, Dictionary<Type, Func<object, object>>> _explicitConversionsTable =
        new()
        {
            // source: https://msdn.microsoft.com/en-us/library/yht2cx7b.aspx
            {
                typeof(sbyte), new Dictionary<Type, Func<object, object>>
                {
                    { typeof(byte), static x => checked((byte)(sbyte)x) },
                    { typeof(ushort), static x => checked((ushort)(sbyte)x) },
                    { typeof(uint), static x => checked((uint)(sbyte)x) },
                    { typeof(ulong), static x => checked((ulong)(sbyte)x) },
                    { typeof(char), static x => checked((char)(sbyte)x) },
                    { typeof(System.Numerics.BigInteger), static x => checked((System.Numerics.BigInteger)(sbyte)x) },
#if NET7_0_OR_GREATER
                    // source: https://learn.microsoft.com/en-us/dotnet/api/system.int128.op_explicit
                    { typeof(UInt128), static x => checked((UInt128)(sbyte)x) },
#endif // NET7_0_OR_GREATER
                }
            },
            {
                typeof(byte), new Dictionary<Type, Func<object, object>>
                {
                    { typeof(sbyte), static x => checked((sbyte)(byte)x) },
                    { typeof(char), static x => checked((char)(byte)x) },
                    { typeof(System.Numerics.BigInteger), static x => checked((System.Numerics.BigInteger)(byte)x) },
                }
            },
            {
                typeof(short), new Dictionary<Type, Func<object, object>>
                {
                    { typeof(sbyte), static x => checked((sbyte)(short)x) },
                    { typeof(byte), static x => checked((byte)(short)x) },
                    { typeof(ushort), static x => checked((ushort)(short)x) },
                    { typeof(uint), static x => checked((uint)(short)x) },
                    { typeof(ulong), static x => checked((ulong)(short)x) },
                    { typeof(char), static x => checked((char)(short)x) },
                    { typeof(System.Numerics.BigInteger), static x => checked((System.Numerics.BigInteger)(short)x) },
#if NET7_0_OR_GREATER
                    // source: https://learn.microsoft.com/en-us/dotnet/api/system.int128.op_explicit
                    { typeof(UInt128), static x => checked((UInt128)(short)x) },
#endif // NET7_0_OR_GREATER
                }
            },
            {
                typeof(ushort), new Dictionary<Type, Func<object, object>>
                {
                    { typeof(sbyte), static x => checked((sbyte)(ushort)x) },
                    { typeof(byte), static x => checked((byte)(ushort)x) },
                    { typeof(short), static x => checked((short)(ushort)x) },
                    { typeof(char), static x => checked((char)(ushort)x) },
                    { typeof(System.Numerics.BigInteger), static x => checked((System.Numerics.BigInteger)(ushort)x) },
                }
            },
            {
                typeof(int), new Dictionary<Type, Func<object, object>>
                {
                    { typeof(sbyte), static x => checked((sbyte)(int)x) },
                    { typeof(byte), static x => checked((byte)(int)x) },
                    { typeof(short), static x => checked((short)(int)x) },
                    { typeof(ushort), static x => checked((ushort)(int)x) },
                    { typeof(uint), static x => checked((uint)(int)x) },
                    { typeof(ulong), static x => checked((ulong)(int)x) },
                    { typeof(char), static x => checked((char)(int)x) },
                    { typeof(System.Numerics.BigInteger), static x => checked((System.Numerics.BigInteger)(int)x) },
#if NET7_0_OR_GREATER
                    // source: https://learn.microsoft.com/en-us/dotnet/api/system.int128.op_explicit
                    { typeof(UInt128), static x => checked((UInt128)(int)x) },
#endif // NET7_0_OR_GREATER
                }
            },
            {
                typeof(uint), new Dictionary<Type, Func<object, object>>
                {
                    { typeof(sbyte), static x => checked((sbyte)(uint)x) },
                    { typeof(byte), static x => checked((byte)(uint)x) },
                    { typeof(short), static x => checked((short)(uint)x) },
                    { typeof(ushort), static x => checked((ushort)(uint)x) },
                    { typeof(int), static x => checked((int)(uint)x) },
                    { typeof(char), static x => checked((char)(uint)x) },
                }
            },
            {
                typeof(long), new Dictionary<Type, Func<object, object>>
                {
                    { typeof(sbyte), static x => checked((sbyte)(long)x) },
                    { typeof(byte), static x => checked((byte)(long)x) },
                    { typeof(short), static x => checked((short)(long)x) },
                    { typeof(ushort), static x => checked((ushort)(long)x) },
                    { typeof(int), static x => checked((int)(long)x) },
                    { typeof(uint), static x => checked((uint)(long)x) },
                    { typeof(ulong), static x => checked((ulong)(long)x) },
                    { typeof(char), static x => checked((char)(long)x) },
                    { typeof(System.Numerics.BigInteger), static x => checked((System.Numerics.BigInteger)(long)x) },
#if NET7_0_OR_GREATER
                    // source: https://learn.microsoft.com/en-us/dotnet/api/system.int128.op_explicit
                    { typeof(UInt128), static x => checked((UInt128)(long)x) },
#endif // NET7_0_OR_GREATER
                }
            },
            {
                typeof(ulong), new Dictionary<Type, Func<object, object>>
                {
                    { typeof(sbyte), static x => checked((sbyte)(ulong)x) },
                    { typeof(byte), static x => checked((byte)(ulong)x) },
                    { typeof(short), static x => checked((short)(ulong)x) },
                    { typeof(ushort), static x => checked((ushort)(ulong)x) },
                    { typeof(int), static x => checked((int)(ulong)x) },
                    { typeof(uint), static x => checked((uint)(ulong)x) },
                    { typeof(long), static x => checked((long)(ulong)x) },
                    { typeof(char), static x => checked((char)(ulong)x) },
                    { typeof(System.Numerics.BigInteger), static x => checked((System.Numerics.BigInteger)(ulong)x) },
                }
            },
            {
                typeof(char), new Dictionary<Type, Func<object, object>>
                {
                    { typeof(sbyte), static x => checked((sbyte)(char)x) },
                    { typeof(byte), static x => checked((byte)(char)x) },
                    { typeof(short), static x => checked((short)(char)x) },
                    { typeof(System.Numerics.BigInteger), static x => checked((System.Numerics.BigInteger)(char)x) },
                }
            },
            {
                typeof(float), new Dictionary<Type, Func<object, object>>
                {
                    { typeof(sbyte), static x => checked((sbyte)(float)x) },
                    { typeof(byte), static x => checked((byte)(float)x) },
                    { typeof(short), static x => checked((short)(float)x) },
                    { typeof(ushort), static x => checked((ushort)(float)x) },
                    { typeof(int), static x => checked((int)(float)x) },
                    { typeof(uint), static x => checked((uint)(float)x) },
                    { typeof(long), static x => checked((long)(float)x) },
                    { typeof(ulong), static x => checked((ulong)(float)x) },
                    { typeof(char), static x => checked((char)(float)x) },
                    { typeof(decimal), static x => checked((decimal)(float)x) },
                    { typeof(System.Numerics.BigInteger), static x => checked((System.Numerics.BigInteger)(float)x) },
#if NET7_0_OR_GREATER
                    { typeof(Int128), static x => checked((Int128)(float)x) },

                    // source: https://learn.microsoft.com/en-us/dotnet/api/system.int128.op_explicit
                    { typeof(UInt128), static x => checked((UInt128)(float)x) },
#endif // NET7_0_OR_GREATER
                }
            },
            {
                typeof(double), new Dictionary<Type, Func<object, object>>
                {
                    { typeof(sbyte), static x => checked((sbyte)(double)x) },
                    { typeof(byte), static x => checked((byte)(double)x) },
                    { typeof(short), static x => checked((short)(double)x) },
                    { typeof(ushort), static x => checked((ushort)(double)x) },
                    { typeof(int), static x => checked((int)(double)x) },
                    { typeof(uint), static x => checked((uint)(double)x) },
                    { typeof(long), static x => checked((long)(double)x) },
                    { typeof(ulong), static x => checked((ulong)(double)x) },
                    { typeof(char), static x => checked((char)(double)x) },
                    { typeof(float), static x => checked((float)(double)x) },
                    { typeof(decimal), static x => checked((decimal)(double)x) },
                    { typeof(System.Numerics.BigInteger), static x => checked((System.Numerics.BigInteger)(double)x) },
#if NET7_0_OR_GREATER
                    // source: https://learn.microsoft.com/en-us/dotnet/api/system.int128.op_explicit
                    { typeof(Int128), static x => checked((Int128)(double)x) },

                    // source: https://learn.microsoft.com/en-us/dotnet/api/system.int128.op_explicit
                    { typeof(UInt128), static x => checked((UInt128)(double)x) },
#endif // NET7_0_OR_GREATER
                }
            },
            {
                typeof(decimal), new Dictionary<Type, Func<object, object>>
                {
                    { typeof(sbyte), static x => checked((sbyte)(decimal)x) },
                    { typeof(byte), static x => checked((byte)(decimal)x) },
                    { typeof(short), static x => checked((short)(decimal)x) },
                    { typeof(ushort), static x => checked((ushort)(decimal)x) },
                    { typeof(int), static x => checked((int)(decimal)x) },
                    { typeof(uint), static x => checked((uint)(decimal)x) },
                    { typeof(long), static x => checked((long)(decimal)x) },
                    { typeof(ulong), static x => checked((ulong)(decimal)x) },
                    { typeof(char), static x => checked((char)(decimal)x) },
                    { typeof(float), static x => checked((float)(decimal)x) },
                    { typeof(double), static x => checked((double)(decimal)x) },
                    { typeof(System.Numerics.BigInteger), static x => checked((System.Numerics.BigInteger)(decimal)x) },
#if NET7_0_OR_GREATER
                    // source: https://learn.microsoft.com/en-us/dotnet/api/system.int128.op_explicit
                    { typeof(Int128), static x => checked((Int128)(decimal)x) },

                    // source: https://learn.microsoft.com/en-us/dotnet/api/system.int128.op_explicit
                    { typeof(UInt128), static x => checked((UInt128)(decimal)x) },
#endif // NET7_0_OR_GREATER
                }
            },
            {
                typeof(System.Numerics.BigInteger), new Dictionary<Type, Func<object, object>>
                {
                    { typeof(sbyte), static x => checked((sbyte)(System.Numerics.BigInteger)x) },
                    { typeof(byte), static x => checked((byte)(System.Numerics.BigInteger)x) },
                    { typeof(short), static x => checked((short)(System.Numerics.BigInteger)x) },
                    { typeof(ushort), static x => checked((ushort)(System.Numerics.BigInteger)x) },
                    { typeof(int), static x => checked((int)(System.Numerics.BigInteger)x) },
                    { typeof(uint), static x => checked((uint)(System.Numerics.BigInteger)x) },
                    { typeof(long), static x => checked((long)(System.Numerics.BigInteger)x) },
                    { typeof(ulong), static x => checked((ulong)(System.Numerics.BigInteger)x) },
                    { typeof(char), static x => checked((char)(System.Numerics.BigInteger)x) },
                    { typeof(float), static x => checked((float)(System.Numerics.BigInteger)x) },
                    { typeof(double), static x => checked((double)(System.Numerics.BigInteger)x) },
                    { typeof(decimal), static x => checked((decimal)(System.Numerics.BigInteger)x) },
                }
            },
            {
                typeof(DateTime), new Dictionary<Type, Func<object, object>>
                {
                    {
                        typeof(DateTimeOffset),
                        static x =>
                        {
                            var d = (DateTime)x;
                            return new DateTimeOffset(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second, d.Millisecond, default);
                        }
                    },
                }
            },
#if NET5_0_OR_GREATER
            {
                typeof(Half), new Dictionary<Type, Func<object, object>>
                {
                    { typeof(sbyte), static x => checked((sbyte)(Half)x) },
                    { typeof(byte), static x => checked((byte)(Half)x) },
                    { typeof(short), static x => checked((short)(Half)x) },
                    { typeof(ushort), static x => checked((ushort)(Half)x) },
                    { typeof(int), static x => checked((int)(Half)x) },
                    { typeof(uint), static x => checked((uint)(Half)x) },
                    { typeof(long), static x => checked((long)(Half)x) },
                    { typeof(ulong), static x => checked((ulong)(Half)x) },
                    { typeof(char), static x => checked((char)(Half)x) },
                    { typeof(float), static x => checked((float)(Half)x) },
                    { typeof(double), static x => checked((double)(Half)x) },
                }
            },
#endif // NET5_0_OR_GREATER
#if NET7_0_OR_GREATER
            {
                // source: https://learn.microsoft.com/en-us/dotnet/api/system.int128.op_explicit
                typeof(Int128), new Dictionary<Type, Func<object, object>>
                {
                    { typeof(sbyte), static x => checked((sbyte)(Int128)x) },
                    { typeof(byte), static x => checked((byte)(Int128)x) },
                    { typeof(short), static x => checked((short)(Int128)x) },
                    { typeof(ushort), static x => checked((ushort)(Int128)x) },
                    { typeof(int), static x => checked((int)(Int128)x) },
                    { typeof(uint), static x => checked((uint)(Int128)x) },
                    { typeof(long), static x => checked((long)(Int128)x) },
                    { typeof(ulong), static x => checked((ulong)(Int128)x) },
                    { typeof(char), static x => checked((char)(Int128)x) },
                    { typeof(float), static x => checked((float)(Int128)x) },
                    { typeof(double), static x => checked((double)(Int128)x) },
                    { typeof(decimal), static x => checked((decimal)(Int128)x) },
                    { typeof(Half), static x => checked((decimal)(Half)x) },
                    { typeof(UInt128), static x => checked((UInt128)(Int128)x) },
                }
            },
            {
                // source: https://learn.microsoft.com/en-us/dotnet/api/system.uint128.op_explicit
                typeof(UInt128), new Dictionary<Type, Func<object, object>>
                {
                    { typeof(sbyte), static x => checked((sbyte)(UInt128)x) },
                    { typeof(byte), static x => checked((byte)(UInt128)x) },
                    { typeof(short), static x => checked((short)(UInt128)x) },
                    { typeof(ushort), static x => checked((ushort)(UInt128)x) },
                    { typeof(int), static x => checked((int)(UInt128)x) },
                    { typeof(uint), static x => checked((uint)(UInt128)x) },
                    { typeof(long), static x => checked((long)(UInt128)x) },
                    { typeof(ulong), static x => checked((ulong)(UInt128)x) },
                    { typeof(char), static x => checked((char)(UInt128)x) },
                    { typeof(float), static x => checked((float)(UInt128)x) },
                    { typeof(double), static x => checked((double)(UInt128)x) },
                    { typeof(decimal), static x => checked((decimal)(UInt128)x) },
                    { typeof(Half), static x => checked((decimal)(Half)x) },
                    { typeof(Int128), static x => checked((Int128)(UInt128)x) },
                }
            },
#endif // NET7_0_OR_GREATER
        };

    private static readonly MethodInfo ToDictionaryMethodInfo = typeof(DynamicObjectMapper).GetMethodEx(nameof(ToDictionary));
    private static readonly MethodInfo GetDefaultValueMethodInfo = typeof(DynamicObjectMapper).GetMethodEx(nameof(GetDefaultValue));

    private readonly DynamicObjectMapperSettings _settings;
    private readonly FromContext _fromContext;
    private readonly ToContext _toContext;
    private readonly ITypeResolver _typeResolver;
    private readonly Func<Type, bool> _isKnownType;
    private readonly Func<Type?, object, DynamicObject> _createDynamicObject;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicObjectMapper"/> class.
    /// </summary>
    /// <param name="typeResolver">Provides a hook for custom logic for type resolution when mapping from <see cref="DynamicObject"/>.</param>
    /// <param name="typeInfoProvider">Provides a hook for mapping type information when mapping to <see cref="DynamicObject"/>.</param>
    /// <param name="settings">Optional settings for dynamic object mapping.</param>
    /// <param name="isKnownTypeProvider">Optional instance to decide whether a type requires to be mapped into a <see cref="DynamicObject"/>, known types do not get mapped.</param>
    /// <param name="typeSafetyChecker">Optional instance to check types prior instance creation.</param>
    public DynamicObjectMapper(ITypeResolver? typeResolver, ITypeInfoProvider? typeInfoProvider, DynamicObjectMapperSettings? settings = null, IIsKnownTypeProvider? isKnownTypeProvider = null, ITypeSafetyChecker? typeSafetyChecker = null)
        : this(settings, typeResolver, null, new InternalDynamicObjectFactory(typeInfoProvider), isKnownTypeProvider, typeSafetyChecker)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicObjectMapper"/> class.
    /// </summary>
    /// <param name="settings">Optional settings for dynamic object mapping.</param>
    /// <param name="typeResolver">Optional instance to be used to resolve types.</param>
    /// <param name="typeMapper">This optional parameter allows mapping type information which get set into the <see cref="DynamicObject"/>s upon their creation.</param>
    /// <param name="dynamicObjectFactory">This optional parameter allows injection of a custom factory for <see cref="DynamicObject"/>.</param>
    /// <param name="isKnownTypeProvider">Optional instance to decide whether a type requires to be mapped into a <see cref="DynamicObject"/>, known types do not get mapped.</param>
    /// <param name="typeSafetyChecker">Optional instance to check types prior instance creation.</param>
    public DynamicObjectMapper(DynamicObjectMapperSettings? settings = null, ITypeResolver? typeResolver = null, ITypeMapper? typeMapper = null, IDynamicObjectFactory? dynamicObjectFactory = null, IIsKnownTypeProvider? isKnownTypeProvider = null, ITypeSafetyChecker? typeSafetyChecker = null)
    {
        _settings = settings?.Copy() ?? new DynamicObjectMapperSettings();

        _fromContext = new FromContext(typeSafetyChecker);

        _toContext = new ToContext(typeMapper);

        _typeResolver = typeResolver ?? TypeResolver.Instance;

        _isKnownType = isKnownTypeProvider is null
            ? static _ => false
            : isKnownTypeProvider.IsKnownType;

        _createDynamicObject = (dynamicObjectFactory ?? new InternalDynamicObjectFactory(null)).CreateDynamicObject;
    }

    /// <summary>
    /// Maps a <see cref="DynamicObject"/> into an instance of the actual type represented by the dynamic object.
    /// </summary>
    /// <param name="obj"><see cref="DynamicObject"/> to be mapped.</param>
    /// <param name="targetType">Target type for mapping, set this parameter to <see langword="null"/> if type information included within <see cref="DynamicObject"/> should be used.</param>
    /// <returns>The object created based on the <see cref="DynamicObject"/> specified.</returns>
    public object? Map(DynamicObject? obj, Type? targetType = null)
    {
        if (obj is null)
        {
            return null;
        }

        if (targetType is null)
        {
            var typeInfo = obj.Type ?? throw new DynamicObjectMapperException("Type property must not be null if no target type specified to mapping method.");
            targetType = typeInfo.ResolveType(_typeResolver);
        }

        return Wrap(() => MapFromDynamicObjectGraph(obj, targetType), _fromContext);
    }

    /// <summary>
    /// Mapps the specified instance into a <see cref="DynamicObject"/>.
    /// </summary>
    /// <remarks>Null references and <see cref="DynamicObject"/> are not mapped.</remarks>
    /// <param name="obj">The instance to be mapped.</param>
    /// <param name="setTypeInformation">Set this parameter to <see langword="true"/> if type information should be included within the <see cref="DynamicObject"/>,
    /// set it to <see langword="false"/> otherwise.</param>
    /// <returns>An instance of <see cref="DynamicObject"/> representing the mapped instance.</returns>
    [return: NotNullIfNotNull(nameof(obj))]
    public DynamicObject? MapObject(object? obj, Func<Type, bool>? setTypeInformation = null)
        => Wrap(() => MapToDynamicObjectGraph(obj, setTypeInformation ?? (static _ => true)), _toContext);

    /// <summary>
    /// Maps an item of an object graph of <see cref="DynamicObject"/> back into its normal representation.
    /// May be overridden in a derived class to implement a customized mapping strategy.
    /// </summary>
    protected virtual object? MapFromDynamicObjectGraph(object? obj, Type? targetType) => MapFromDynamicObjectIfRequired(obj, targetType);

    /// <summary>
    /// Maps an item of an object graph into a <see cref="DynamicObject"/>.
    /// May be overridden in a derived class to implement a customized mapping strategy.
    /// </summary>
    [return: NotNullIfNotNull(nameof(obj))]
    protected virtual DynamicObject? MapToDynamicObjectGraph(object? obj, Func<Type, bool> setTypeInformation) => MapInternal(obj, setTypeInformation);

    /// <summary>
    /// When overridden in a derived class, determines whether a collection should be mapped into a single <see cref="DynamicObject"/>,
    /// rather than into a collection of <see cref="DynamicObject"/>s. Default is <see langword="false"/>.
    /// </summary>
    /// <returns><see langword="true"/> if the collection should be mapped into a single <see cref="DynamicObject"/>,
    /// <see langword="false"/> if each element should be mapped separately. Default is <see langword="false"/>.</returns>
    protected virtual bool ShouldMapToDynamicObject(IEnumerable collection) => false;

    private object? MapFromDynamicObjectIfRequired(object? obj, Type? targetType)
    {
        if (obj is null)
        {
            return null;
        }

        var resultType = targetType;
        if (obj is DynamicObject dynamicObj)
        {
            object? MapRequired(object? o, Type t) => MapFromDynamicObjectIfRequired(o, t);
            TType? MapToTypeInfo<TType>() => (TType?)MapRequired(dynamicObj, typeof(TType));

            var sourceType = dynamicObj.Type.ResolveType(_typeResolver);
            if (sourceType is not null)
            {
                if ((resultType == typeof(Type) || resultType == typeof(TypeInfo)) &&
                    (sourceType == typeof(TypeSystem.TypeInfo) || sourceType == typeof(Type) || sourceType == typeof(TypeInfo)))
                {
                    var typeInfo = MapToTypeInfo<TypeSystem.TypeInfo>();
                    var t = typeInfo.ResolveType(_typeResolver);
                    return resultType == typeof(TypeInfo) ? t?.GetTypeInfo() : t;
                }

                if (resultType == typeof(MethodInfo) && (sourceType == typeof(TypeSystem.MethodInfo) || sourceType == typeof(MethodInfo)))
                {
                    var methodInfo = MapToTypeInfo<TypeSystem.MethodInfo>();
                    return methodInfo.ResolveMemberInfo(_typeResolver);
                }

                if (resultType == typeof(PropertyInfo) && (sourceType == typeof(TypeSystem.PropertyInfo) || sourceType == typeof(PropertyInfo)))
                {
                    var propertyInfo = MapToTypeInfo<TypeSystem.PropertyInfo>();
                    return propertyInfo.ResolveMemberInfo(_typeResolver);
                }

                if (resultType == typeof(FieldInfo) && (sourceType == typeof(TypeSystem.FieldInfo) || sourceType == typeof(FieldInfo)))
                {
                    var fieldInfo = MapToTypeInfo<TypeSystem.FieldInfo>();
                    return fieldInfo.ResolveMemberInfo(_typeResolver);
                }

                if (resultType == typeof(ConstructorInfo) && (sourceType == typeof(TypeSystem.ConstructorInfo) || sourceType == typeof(ConstructorInfo)))
                {
                    var constructorInfo = MapToTypeInfo<TypeSystem.ConstructorInfo>();
                    return constructorInfo.ResolveConstructor(_typeResolver);
                }

                if (resultType is null || resultType.IsAssignableFrom(sourceType))
                {
                    resultType = sourceType;
                }
            }

            if (resultType is null)
            {
                return dynamicObj;
            }

            if (dynamicObj.IsNull)
            {
                return GetDefault(resultType);
            }

            if (resultType.IsAssignableFrom(typeof(DynamicObject)) && resultType != typeof(object))
            {
                return dynamicObj;
            }

            if (dynamicObj.IsSingleValueWrapper())
            {
                return MapRequired(dynamicObj.GetValues().Single(), resultType);
            }

            return MapInternal(dynamicObj, sourceType, resultType);
        }

        if (resultType is null)
        {
            return obj;
        }

        var objectType = obj.GetType();
        if (objectType == resultType && !obj.IsCollection(out _))
        {
            return obj;
        }

        if (resultType.IsEnum())
        {
            if (resultType.IsAssignableFrom(objectType))
            {
                return obj;
            }

            var targetEnumType = resultType.AsNonNullableType();

            if (obj is string str)
            {
                return Enum.Parse(targetEnumType, str, true);
            }

            return Enum.ToObject(targetEnumType, obj);
        }

        if (resultType.IsAssignableFrom(objectType) && _isKnownType(objectType))
        {
            return obj;
        }

        if (_isNativeType(resultType))
        {
            if (obj is string str)
            {
                return ParseToNativeType(resultType.AsNonNullableType(), str);
            }

            if (objectType == resultType)
            {
                return obj;
            }
        }

        if (obj.IsCollection(out var collection))
        {
            var elementType = TypeHelper.GetElementType(resultType);

            var items = collection
                .Cast<object>()
                .Select(x => MapFromDynamicObjectGraph(x, elementType))
                .ToList();
            var r1 = MethodInfos.Enumerable.Cast.MakeGenericMethod(elementType).Invoke(null, new[] { items });

            if (resultType.IsArray)
            {
                return MethodInfos.Enumerable.ToArray.MakeGenericMethod(elementType).Invoke(null, [r1]);
            }

            if (IsMatchingDictionary(resultType, elementType))
            {
                var targetTypeGenericArguments = resultType.GetGenericArguments();
                var method = ToDictionaryMethodInfo.MakeGenericMethod(targetTypeGenericArguments);
                return method.Invoke(null, [r1]);
            }

            if (resultType.IsAssignableFrom(typeof(List<>).MakeGenericType(elementType)))
            {
                return MethodInfos.Enumerable.ToList.MakeGenericMethod(elementType).Invoke(null, [r1]);
            }

            if (resultType.IsAssignableFrom(typeof(IQueryable<>).MakeGenericType(elementType)))
            {
                return MethodInfos.Queryable.AsQueryable.MakeGenericMethod(elementType).Invoke(null, [r1]);
            }

            var enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);
            var ctor = resultType.GetConstructors().FirstOrDefault(c =>
            {
                var parameters = c.GetParameters();
                return parameters.Length is 1
                    && parameters[0].ParameterType.IsAssignableFrom(enumerableType);
            });

            if (ctor is not null)
            {
                return ctor.Invoke([r1]);
            }

            if (enumerableType.IsAssignableFrom(resultType))
            {
                ctor = resultType.GetConstructors().FirstOrDefault(static c => c.GetParameters().Length is 0);
                if (ctor is not null)
                {
                    var addMethod = resultType.GetMethods()
                        .Where(static m => string.Equals(m.Name, "Add", StringComparison.Ordinal))
                        .FirstOrDefault(m =>
                        {
                            var parameters = m.GetParameters();
                            return parameters.Length is 1
                                && parameters[0].ParameterType.IsAssignableFrom(elementType);
                        });
                    if (addMethod is not null)
                    {
                        var targetCollection = ctor.Invoke(null);
                        Helper.AddAllMethodInfo.MakeGenericMethod(elementType).Invoke(null, [r1, targetCollection, addMethod]);
                        return targetCollection;
                    }
                }
            }

            throw new DynamicObjectMapperException($"Failed to project collection with element type '{elementType?.AssemblyQualifiedName}' into type '{resultType.AssemblyQualifiedName}'");
        }

        if (targetType == typeof(Type) && obj is TypeSystem.TypeInfo typeInfoObj)
        {
            return typeInfoObj.ResolveType(_typeResolver);
        }

        return obj;
    }

    /// <summary>
    /// Maps from object to dynamic object if required.
    /// </summary>
    /// <remarks>Null references, strings, value types, and dynamic objects are no mapped.</remarks>
    [return: NotNullIfNotNull(nameof(obj))]
    private object? MapToDynamicObjectIfRequired(object? obj, Func<Type, bool> setTypeInformation)
    {
        if (obj is null)
        {
            return _settings.WrapNullAsDynamicObject
                ? DynamicObject.CreateDefault()
                : null;
        }

        if (obj is DynamicObject or string)
        {
            return obj;
        }

        var type = obj.GetType();
        if (_isKnownType(type))
        {
            return obj;
        }

        if (_isNativeType(type))
        {
            return _settings.FormatNativeTypesAsString ? FormatNativeTypeAsString(obj, type) : obj;
        }

        if (type.IsEnum())
        {
            return obj.ToString() ?? string.Empty;
        }

        if (obj is IEnumerable collection && !ShouldMapToDynamicObject(collection))
        {
            var items = collection
                .Cast<object>()
                .Select(x => MapToDynamicObjectIfRequired(x, setTypeInformation))
                .ToArray();

            var elementType = TypeHelper.GetElementType(type);
            if (elementType != typeof(object))
            {
                if (_isKnownType(elementType))
                {
                    // keep unchanged
                }
                else if (elementType.IsEnum() || (_settings.FormatNativeTypesAsString && _isNativeType(elementType)))
                {
                    elementType = typeof(string);
                }
                else if (items.All(static x => x is null or DynamicObject))
                {
                    elementType = typeof(DynamicObject);
                }

                return items.CastCollectionToArrayOfType(elementType);
            }

            return items;
        }

        if (_settings.PassthroughAquaTypeSystemTypes &&
            (obj is TypeSystem.TypeInfo or TypeSystem.MemberInfo))
        {
            return obj;
        }

        return MapToDynamicObjectGraph(obj, setTypeInformation);
    }

    /// <summary>
    /// Extrancts member values from source object and populates to dynamic object.
    /// </summary>
    private void PopulateObjectMembers(Type type, object from, DynamicObject to, Func<Type, bool> setTypeInformation)
    {
#if !NET8_0_OR_GREATER
        if (_settings.UtilizeFormatterServices && type.IsSerializable)
        {
            MapObjectMembers(type, from, to, setTypeInformation);
        }
        else
        {
#endif // NET8_0_OR_GREATER
            var properties = GetPropertiesForMapping(type) ?? type.GetDefaultPropertiesForSerialization();
            foreach (var property in properties.Where(HasNoUnmappedAnnotation))
            {
                var value = property.GetValue(from);
                value = MapToDynamicObjectIfRequired(value, setTypeInformation);
                to.Add(property.Name, value);
            }

            var fields = GetFieldsForMapping(type) ?? type.GetDefaultFieldsForSerialization();
            foreach (var field in fields.Where(HasNoUnmappedAnnotation))
            {
                var value = field.GetValue(from);
                value = MapToDynamicObjectIfRequired(value, setTypeInformation);
                to.Add(field.Name, value);
            }
#if !NET8_0_OR_GREATER
        }
#endif // NET8_0_OR_GREATER
    }

    /// <summary>
    /// Can be overriden in a derived class to return a list of <see cref="PropertyInfo"/> for a given type or <see langword="null"/> if defaul behaviour should be applied.
    /// </summary>
    /// <returns>If overriden in a derived class, returns a list of <see cref="PropertyInfo"/> for a given type or <see langword="null"/> if defaul behaviour should be applied.</returns>
    [SuppressMessage("Major Code Smell", "S1168:Empty arrays and collections should be returned instead of null", Justification = "Null has special meaning")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:Element parameters should be documented", Justification = "Null has special meaning")]
    protected virtual IEnumerable<PropertyInfo>? GetPropertiesForMapping(Type type) => null;

    /// <summary>
    /// Can be overriden in a derived class to return a list of <see cref="FieldInfo"/> for a given type or <see langword="null"/> if defaul behaviour should be applied.
    /// </summary>
    /// <returns>If overriden in a derived class, returns a list of <see cref="FieldInfo"/> for a given type or <see langword="null"/> if defaul behaviour should be applied.</returns>
    [SuppressMessage("Major Code Smell", "S1168:Empty arrays and collections should be returned instead of null", Justification = "Null has special meaning")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:Element parameters should be documented", Justification = "Null has special meaning")]
    protected virtual IEnumerable<FieldInfo>? GetFieldsForMapping(Type type) => null;

    /// <summary>
    /// Can be overriden in a derived class to return a cutom object factory for a given type.
    /// </summary>
    /// <param name="targetType">The type of the instance to be created.</param>
    /// <param name="dynamicObject">Dynamic object holding data for the instance to be created.</param>
    /// <param name="factory">The factory function or <see langword="null"/> if <see langword="false"/> is returned as result.</param>
    /// <returns><see langword="true"/> if a factory function is assigned to the out parameter, <see langword="false"/> otherwise.</returns>
    protected virtual bool TryGetObjectFactory(Type targetType, DynamicObject dynamicObject, [NotNullWhen(true)] out Func<object>? factory)
    {
        factory = null;
        return false;
    }

    /// <summary>
    /// Can be overriden in a derived class to return a cutom object initializer for a given instance.
    /// </summary>
    /// <param name="targetType">The type of the instance to be initalized.</param>
    /// <param name="dynamicObject">Dynamic object holding data for the instance to be initalized.</param>
    /// <param name="initializer">The initializer function or <see langword="null"/> if <see langword="false"/> is returned as result.</param>
    /// <returns><see langword="true"/> if an initializer function is assigned to the out parameter, <see langword="false"/> otherwise.</returns>
    protected virtual bool TryGetObjectInitializer(Type targetType, DynamicObject dynamicObject, [NotNullWhen(true)] out Action<object>? initializer)
    {
        initializer = null;
        return false;
    }

    /// <summary>
    /// Maps an object to a dynamic object.
    /// </summary>
    /// <remarks>Null references and dynamic objects are not mapped.</remarks>
    [return: NotNullIfNotNull(nameof(obj))]
    private DynamicObject? MapInternal(object? obj, Func<Type, bool> setTypeInformation)
    {
        if (obj is null)
        {
            return _settings.WrapNullAsDynamicObject
                ? DynamicObject.CreateDefault()
                : null;
        }

        if (obj is DynamicObject dynamicObject)
        {
            return dynamicObject;
        }

        Func<Type?, object, Func<Type, bool>, DynamicObject> facotry;
        Action<Type, object, DynamicObject, Func<Type, bool>>? initializer = null;

        var sourceType = obj.GetType();
        if (_isNativeType(sourceType) || _isKnownType(sourceType) || sourceType.IsEnum())
        {
            facotry = (t, o, f) =>
            {
                var value = MapToDynamicObjectIfRequired(o, f);
                var dynamicObject = _createDynamicObject(t, o);
                dynamicObject.Add(string.Empty, value);
                return dynamicObject;
            };
        }
        else if (obj.IsCollection(out var collection) && !ShouldMapToDynamicObject(collection))
        {
            facotry = (t, o, f) =>
            {
                var list = ((IEnumerable)o)
                    .Cast<object>()
                    .Select(x => MapToDynamicObjectIfRequired(x, f))
                    .ToArray();
                var dynamicObject = _createDynamicObject(t, o);
                dynamicObject.Add(string.Empty, list);
                return dynamicObject;
            };
        }
        else if (obj is Type t)
        {
            var typeInfo = new Lazy<TypeSystem.TypeInfo>(() => new TypeSystem.TypeInfo(t, false, false));
            facotry = (t, o, f) => _createDynamicObject(typeof(Type), typeInfo.Value);
            initializer = (t, o, to, f) => PopulateObjectMembers(typeof(TypeSystem.TypeInfo), typeInfo.Value, to, f);
        }
        else if (obj is TypeInfo ti)
        {
            var typeInfo = new Lazy<TypeSystem.TypeInfo>(() => new TypeSystem.TypeInfo(ti.AsType(), false, false));
            facotry = (t, o, f) => _createDynamicObject(typeof(TypeInfo), typeInfo.Value);
            initializer = (t, o, to, f) => PopulateObjectMembers(typeof(TypeSystem.TypeInfo), typeInfo.Value, to, f);
        }
        else if (obj is MethodInfo mi)
        {
            var methodInfo = new Lazy<TypeSystem.MethodInfo>(() => new TypeSystem.MethodInfo(mi));
            facotry = (t, o, f) => _createDynamicObject(typeof(MethodInfo), methodInfo.Value);
            initializer = (t, o, to, f) => PopulateObjectMembers(typeof(TypeSystem.MethodInfo), methodInfo.Value, to, f);
        }
        else if (obj is PropertyInfo pi)
        {
            var propertyInfo = new Lazy<TypeSystem.PropertyInfo>(() => new TypeSystem.PropertyInfo(pi));
            facotry = (t, o, f) => _createDynamicObject(typeof(PropertyInfo), propertyInfo.Value);
            initializer = (t, o, to, f) => PopulateObjectMembers(typeof(TypeSystem.PropertyInfo), propertyInfo.Value, to, f);
        }
        else if (obj is FieldInfo fi)
        {
            var fieldInfo = new Lazy<TypeSystem.FieldInfo>(() => new TypeSystem.FieldInfo(fi));
            facotry = (t, o, f) => _createDynamicObject(typeof(FieldInfo), fieldInfo.Value);
            initializer = (t, o, to, f) => PopulateObjectMembers(typeof(TypeSystem.FieldInfo), fieldInfo.Value, to, f);
        }
        else if (obj is ConstructorInfo ci)
        {
            var constructorInfo = new Lazy<TypeSystem.ConstructorInfo>(() => new TypeSystem.ConstructorInfo(ci));
            facotry = (t, o, f) => _createDynamicObject(typeof(ConstructorInfo), constructorInfo.Value);
            initializer = (t, o, to, f) => PopulateObjectMembers(typeof(TypeSystem.ConstructorInfo), constructorInfo.Value, to, f);
        }
        else if (obj is Delegate d)
        {
            var methodInfo = new Lazy<TypeSystem.MethodInfo>(() => new TypeSystem.MethodInfo(d.Method));
            facotry = (t, o, f) => _createDynamicObject(typeof(MethodInfo), methodInfo.Value);
            initializer = (t, o, to, f) => PopulateObjectMembers(typeof(TypeSystem.MethodInfo), methodInfo.Value, to, f);
        }
        else
        {
            facotry = (t, o, f) => _createDynamicObject(t, o);
            initializer = PopulateObjectMembers;
        }

        return _toContext.TryGetOrCreateNew(sourceType, obj, facotry, initializer, setTypeInformation);
    }

    private object MapInternal(DynamicObject obj, Type? sourceType, Type targetType)
    {
        // project data record
        Func<Type, DynamicObject, object> factory;
        Action<Type, DynamicObject, object>? initializer = null;

        if (typeof(Delegate).IsAssignableFrom(targetType) && (sourceType == typeof(MethodInfo) || sourceType == typeof(TypeSystem.MethodInfo)))
        {
            factory = (t, o) =>
            {
                var method = (MethodInfo?)MapFromDynamicObjectIfRequired(o, typeof(MethodInfo)) ?? throw new DynamicObjectMapperException($"Missing {nameof(MethodInfo)}");
                if (method.IsStatic)
                {
                    return method.CreateDelegate(t);
                }

                var instance = Activator.CreateInstance(method.DeclaringType!);
                return method.CreateDelegate(t, instance);
            };
        }
#if !NET8_0_OR_GREATER
        else if (_settings.UtilizeFormatterServices && targetType.IsSerializable)
        {
            factory = (t, item) => GetUninitializedObject(t);
            initializer = PopulateObjectMembers;
        }
#endif // NET8_0_OR_GREATER
        else
        {
            if (TryGetObjectFactory(targetType, obj, out var func))
            {
                if (func is null)
                {
                    throw new DynamicObjectMapperException($"Factory function must not be null if {nameof(TryGetObjectFactory)} returns true.");
                }

                factory = (_, _) => func();
            }
            else
            {
                var constructor = targetType.GetConstructors()
                    .Select(i =>
                    {
                        var paramterList = i.GetParameters();
                        return new
                        {
                            Info = i,
                            ParametersCount = paramterList.Length,
                            Parameters = paramterList
                                .Select(parameter => new
                                {
                                    Info = parameter,
                                    Property = obj.Properties?
                                        .Where(dynamicProperty => string.Equals(dynamicProperty.Name, parameter.Name, StringComparison.OrdinalIgnoreCase))
                                        .Select(dynamicProperty => new { dynamicProperty.Name, Value = MapFromDynamicObjectGraph(dynamicProperty.Value, parameter.ParameterType) })
                                        .SingleOrDefault(dynamicProperty => IsAssignable(parameter.ParameterType, dynamicProperty.Value)),
                                })
                                .ToArray(),
                        };
                    })
                    .OrderByDescending(static i => i.ParametersCount is 0 ? int.MaxValue : i.ParametersCount)
                    .FirstOrDefault(static i => i.Parameters.All(static p => p.Property is not null));

                if (constructor is not null)
                {
                    factory = (t, item) =>
                    {
                        var arguments = constructor.Parameters
                            .Select(x => x.Property?.Value)
                            .ToArray();
                        var instance = constructor.Info.Invoke(arguments);
                        return instance;
                    };
                }
#if NET8_0_OR_GREATER
                else if (targetType.IsValueType && targetType.GetCustomAttribute<System.Runtime.CompilerServices.IsReadOnlyAttribute>() is null)
#else
                else if (targetType.IsValueType)
#endif // NET8_0_OR_GREATER
                {
                    factory = static (t, item) => Activator.CreateInstance(t) ?? throw new DynamicObjectMapperException($"Failed to create instance of type {t.FullName}");
                }
                else
                {
                    throw new DynamicObjectMapperException($"Failed to pick matching constructor for type {targetType.FullName}");
                }
            }

            if (TryGetObjectInitializer(targetType, obj, out var init))
            {
                if (func is null)
                {
                    throw new DynamicObjectMapperException($"Initializer function must not be null if {nameof(TryGetObjectInitializer)} returns true.");
                }

                initializer = (_, _, x) => init(x);
            }
            else
            {
                initializer = InitializeProperties;
            }
        }

        return _fromContext.TryGetOrCreateNew(targetType, obj, factory, initializer);
    }

    private void InitializeProperties(Type type, DynamicObject item, object obj)
    {
        var properties = GetPropertiesForMapping(type) ?? type.GetDefaultPropertiesForDeserialization();
        foreach (var property in properties.Where(HasNoUnmappedAnnotation))
        {
            if (item.TryGet(property.Name, out var rawValue))
            {
                var value = MapFromDynamicObjectGraph(rawValue, property.PropertyType);
                if (IsAssignable(property.PropertyType, value) ||
                    TryExplicitConversions(property.PropertyType, ref value) ||
                    !_settings.SilentlySkipUnassignableMembers)
                {
                    property.SetValue(obj, value);
                }
            }
        }

        var fields = GetFieldsForMapping(type) ?? type.GetDefaultFieldsForDeserialization();
        foreach (var field in fields.Where(f => !f.IsInitOnly && HasNoUnmappedAnnotation(f)))
        {
            if (item.TryGet(field.Name, out var rawValue))
            {
                var value = MapFromDynamicObjectGraph(rawValue, field.FieldType);
                if (IsAssignable(field.FieldType, value) ||
                    TryExplicitConversions(field.FieldType, ref value) ||
                    !_settings.SilentlySkipUnassignableMembers)
                {
                    field.SetValue(obj, value);
                }
            }
        }
    }

    private static object ParseToNativeType(Type targetType, string value)
    {
        if (targetType == typeof(string))
        {
            return value;
        }

        if (targetType == typeof(int))
        {
            return int.Parse(value, CultureInfo.InvariantCulture);
        }

        if (targetType == typeof(uint))
        {
            return uint.Parse(value, CultureInfo.InvariantCulture);
        }

        if (targetType == typeof(byte))
        {
            return byte.Parse(value, CultureInfo.InvariantCulture);
        }

        if (targetType == typeof(sbyte))
        {
            return sbyte.Parse(value, CultureInfo.InvariantCulture);
        }

        if (targetType == typeof(short))
        {
            return short.Parse(value, CultureInfo.InvariantCulture);
        }

        if (targetType == typeof(ushort))
        {
            return ushort.Parse(value, CultureInfo.InvariantCulture);
        }

        if (targetType == typeof(long))
        {
            return long.Parse(value, CultureInfo.InvariantCulture);
        }

        if (targetType == typeof(ulong))
        {
            return ulong.Parse(value, CultureInfo.InvariantCulture);
        }

        if (targetType == typeof(float))
        {
            return float.Parse(value, CultureInfo.InvariantCulture);
        }

        if (targetType == typeof(double))
        {
            return double.Parse(value, CultureInfo.InvariantCulture);
        }

        if (targetType == typeof(decimal))
        {
            return decimal.Parse(value, CultureInfo.InvariantCulture);
        }

        if (targetType == typeof(char))
        {
            return char.Parse(value);
        }

        if (targetType == typeof(bool))
        {
            return bool.Parse(value);
        }

        if (targetType == typeof(Guid))
        {
            return Guid.Parse(value);
        }

        if (targetType == typeof(DateTime))
        {
            return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }

        if (targetType == typeof(DateTimeOffset))
        {
            return DateTimeOffset.Parse(value, CultureInfo.InvariantCulture);
        }

        if (targetType == typeof(TimeSpan))
        {
            return TimeSpan.Parse(value, CultureInfo.InvariantCulture);
        }

        if (targetType == typeof(System.Numerics.BigInteger))
        {
            return System.Numerics.BigInteger.Parse(value, CultureInfo.InvariantCulture);
        }

        if (targetType == typeof(System.Numerics.Complex))
        {
            var m = Regex.Match(value, ComplexNumberParserRegexPattern);
            if (m.Success)
            {
                var re = double.Parse(m.Groups["Re"].Value, CultureInfo.InvariantCulture);
                var im = double.Parse(m.Groups["Sign"].Value + m.Groups["Im"].Value, CultureInfo.InvariantCulture);
                return new System.Numerics.Complex(re, im);
            }
            else
            {
                throw new DynamicObjectMapperException(new FormatException($"Value '{value}' cannot be parsed into complex number."));
            }
        }

        if (targetType == typeof(byte[]))
        {
            return Convert.FromBase64String(value);
        }

#if NET5_0_OR_GREATER
        if (targetType == typeof(Half))
        {
            return Half.Parse(value, CultureInfo.InvariantCulture);
        }
#endif // NET5_0_OR_GREATER

#if NET6_0_OR_GREATER
        if (targetType == typeof(DateOnly))
        {
            return DateOnly.Parse(value, CultureInfo.InvariantCulture);
        }

        if (targetType == typeof(TimeOnly))
        {
            return TimeOnly.Parse(value, CultureInfo.InvariantCulture);
        }
#endif // NET6_0_OR_GREATER

#if NET7_0_OR_GREATER
        if (targetType == typeof(Int128))
        {
            return Int128.Parse(value, CultureInfo.InvariantCulture);
        }

        if (targetType == typeof(UInt128))
        {
            return UInt128.Parse(value, CultureInfo.InvariantCulture);
        }
#endif // NET7_0_OR_GREATER

        throw new DynamicObjectMapperException(new NotImplementedException($"string parser for type {targetType} is not implemented"));
    }

    private static bool IsMatchingDictionary(Type targetType, Type elementType)
    {
        if (!(targetType.IsGenericType && elementType.IsGenericType))
        {
            return false;
        }

        var elementTypeGenericTypeDefinition = elementType.GetGenericTypeDefinition();
        if (!_genericKeyValuePairType.IsAssignableFrom(elementTypeGenericTypeDefinition))
        {
            return false;
        }

        var targetTypeGenericArgumentsCount = targetType.GetGenericArguments().Length;
        if (targetTypeGenericArgumentsCount is not 2)
        {
            return false;
        }

        var elementTypeGenericArguments = elementType.GetGenericArguments().ToArray();
        var dictionaryType = _genericDictionaryType.MakeGenericType(elementTypeGenericArguments);
        if (!targetType.IsAssignableFrom(dictionaryType))
        {
            return false;
        }

        return true;
    }

    private static object ToDictionary<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> items)
        where TKey : notnull
        => items.ToDictionary(static x => x.Key, static x => x.Value);

    private static object? GetDefault(Type type)
        => GetDefaultValueMethodInfo
        .MakeGenericMethod(type)
        .Invoke(null, null);

    private static object? GetDefaultValue<T>()
        => default(T);

    private static bool IsAssignable(Type targetType, object? value)
    {
        if (targetType.IsValueType)
        {
            if (value is null)
            {
                return targetType.IsGenericType
                    && targetType.GetGenericTypeDefinition() == typeof(Nullable<>);
            }

            return targetType.IsInstanceOfType(value)
                || HasImplicitNumericConversions(value.GetType(), targetType);
        }

        return value is null
            || targetType.IsInstanceOfType(value);
    }

    private static bool HasImplicitNumericConversions(Type from, Type to)
        => _implicitNumericConversionsTable.TryGetValue(from, out var toList)
        && toList.Contains(to);

    private static bool TryExplicitConversions(Type targetType, ref object? value)
    {
        if (value is null)
        {
            return false;
        }

        var sourceType = value.GetType();
        sourceType = sourceType.AsNonNullableType();
        targetType = targetType.AsNonNullableType();

        if (_explicitConversionsTable.TryGetValue(sourceType, out var converterMap) &&
            converterMap.TryGetValue(targetType, out var converter))
        {
            try
            {
                value = converter(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        if (value is string text)
        {
            if (_isNativeType(targetType))
            {
                value = ParseToNativeType(targetType, text);
                return true;
            }

            if (targetType.IsEnum())
            {
                value = Enum.Parse(targetType, text);
                return true;
            }
        }

        if (targetType.TryDynamicCast(value, out var result))
        {
            value = result;
            return true;
        }

        return false;
    }

    private static string FormatNativeTypeAsString(object obj, Type type)
    {
        if (type == typeof(DateTime) || type == typeof(DateTime?))
        {
            return ((DateTime)obj).ToString("o", CultureInfo.InvariantCulture);
        }

        if (type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?))
        {
            return ((DateTimeOffset)obj).ToString("o", CultureInfo.InvariantCulture);
        }

        if (type == typeof(TimeSpan) || type == typeof(TimeSpan?))
        {
            return ((TimeSpan)obj).ToString("c", CultureInfo.InvariantCulture);
        }

        if (type == typeof(Guid) || type == typeof(Guid?))
        {
            return ((Guid)obj).ToString("D", CultureInfo.InvariantCulture);
        }

        if (type == typeof(float) || type == typeof(float?))
        {
            return ((float)obj).ToString("G9", CultureInfo.InvariantCulture);
        }

        if (type == typeof(double) || type == typeof(double?))
        {
            return ((double)obj).ToString("G17", CultureInfo.InvariantCulture);
        }

        if (type == typeof(decimal) || type == typeof(decimal?))
        {
            return ((decimal)obj).ToString(CultureInfo.InvariantCulture);
        }

        if (type == typeof(System.Numerics.BigInteger) || type == typeof(System.Numerics.BigInteger?))
        {
            return ((System.Numerics.BigInteger)obj).ToString("R", CultureInfo.InvariantCulture);
        }

        if (type == typeof(System.Numerics.Complex) || type == typeof(System.Numerics.Complex?))
        {
            var c = (System.Numerics.Complex)obj;
            return
#if NET8_0_OR_GREATER
                string.Create(
                    CultureInfo.InvariantCulture,
#else
#pragma warning disable S6618 // "string.Create" should be used instead of "FormattableString"
                FormattableString.Invariant(
#pragma warning restore S6618 // "string.Create" should be used instead of "FormattableString"
#endif // NET8_0_OR_GREATER
                    $"{c.Real:R}{Math.Sign(c.Imaginary):+;-}i{Math.Abs(c.Imaginary):R}");
        }

        if (type == typeof(byte[]))
        {
            return Convert.ToBase64String((byte[])obj);
        }

#if NET5_0_OR_GREATER
        if (type == typeof(Half) || type == typeof(Half?))
        {
            return ((Half)obj).ToString("R", CultureInfo.InvariantCulture);
        }
#endif // NET5_0_OR_GREATER

#if NET6_0_OR_GREATER
        if (type == typeof(DateOnly) || type == typeof(DateOnly?))
        {
            return ((DateOnly)obj).ToString("O", CultureInfo.InvariantCulture);
        }

        if (type == typeof(TimeOnly) || type == typeof(TimeOnly?))
        {
            return ((TimeOnly)obj).ToString("O", CultureInfo.InvariantCulture);
        }
#endif // NET6_0_OR_GREATER

#if NET7_0_OR_GREATER
        if (type == typeof(Int128) || type == typeof(Int128?))
        {
            return ((Int128)obj).ToString(CultureInfo.InvariantCulture);
        }

        if (type == typeof(UInt128) || type == typeof(UInt128?))
        {
            return ((UInt128)obj).ToString(CultureInfo.InvariantCulture);
        }
#endif // NET7_0_OR_GREATER

        return obj.ToString() ?? string.Empty;
    }

    private T Wrap<T>(Func<T> func, IMappingContext mappingContext)
    {
        lock (mappingContext)
        {
            try
            {
                return func();
            }
            catch (DynamicObjectMapperException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DynamicObjectMapperException(ex);
            }
            finally
            {
                if (!_settings.PreserveMappingCache)
                {
                    mappingContext.Recycle();
                }
            }
        }
    }
}