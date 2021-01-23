// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Newtonsoft.Json.Converters
{
    using Aqua.Dynamic;
    using Aqua.TypeSystem;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    internal sealed class KnowTypesRegistry
    {
        private static readonly Dictionary<Type, string> _keyLookup;
        private static readonly Dictionary<string, TypeInfo> _typeLookup;

#pragma warning disable S3963 // "static" fields should be initialized inline
        static KnowTypesRegistry()
        {
            _keyLookup = new[]
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
                    typeof(TypeInfo),
                    typeof(DynamicObject),
                }
                .Select(x =>
                {
                    var name = x switch
                    {
                        Type t when t == typeof(TypeInfo) => "type",
                        Type t when t == typeof(DynamicObject) => "dynamic",
                        _ => x.Name.ToLowerInvariant(),
                    };

                    return (Type: x, Key: name);
                })
                .SelectMany(x => new[]
                    {
                        x,
                        (x.Item1.IsValueType ? (typeof(Nullable<>).MakeGenericType(x.Item1), x.Item2 + "?") : x),
                        (x.Item1.MakeArrayType(), x.Item2 + "[]"),
                    })
                .Distinct()
                .ToDictionary(x => x.Item1, x => x.Item2);

            _typeLookup = _keyLookup.ToDictionary(x => x.Value, x => new TypeInfo(x.Key, false, false));
        }
#pragma warning restore S3963 // "static" fields should be initialized inline

        public static KnowTypesRegistry Instance { get; } = new KnowTypesRegistry();

        public bool TryGetTypeInfo(string key, [MaybeNullWhen(false)] out TypeInfo typeInfo)
        {
            typeInfo = _typeLookup.TryGetValue(key, out var type)
                ? new TypeInfo(type)
                : null;
            return typeInfo is not null;
        }

        public bool TryGetTypeKey(TypeInfo type, [MaybeNullWhen(false)] out string typeKey) => TryGetTypeKey(type.ToType(), out typeKey);

        public bool TryGetTypeKey(Type type, [MaybeNullWhen(false)] out string typeKey) => _keyLookup.TryGetValue(type, out typeKey);
    }
}