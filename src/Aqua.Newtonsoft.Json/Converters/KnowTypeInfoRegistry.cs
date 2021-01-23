// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Newtonsoft.Json.Converters
{
    using Aqua.Dynamic;
    using Aqua.TypeSystem;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Numerics;

    internal sealed class KnowTypeInfoRegistry
    {
        private static readonly Dictionary<Type, string> _keyLookup;
        private static readonly Dictionary<string, TypeInfo> _typeLookup;

#pragma warning disable S3963 // "static" fields should be initialized inline
        static KnowTypeInfoRegistry()
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
                    typeof(BigInteger),
                    typeof(Complex),
                    typeof(TypeInfo),
                    typeof(DynamicObject),
                }
                .ToDictionary(x => x, x => x.Name.ToLower());

            _typeLookup = _keyLookup.ToDictionary(x => x.Value, x => new TypeInfo(x.Key, false, false));
        }
#pragma warning restore S3963 // "static" fields should be initialized inline

        public bool TryLookupTypeInfo(string key, [MaybeNullWhen(false)] out TypeInfo typeInfo) => _typeLookup.TryGetValue(key, out typeInfo);

        public bool TryLookupKnownTypeKey(TypeInfo type, [MaybeNullWhen(false)] out string typeKey) => TryLookupKnownTypeKey(type.ToType(), out typeKey);

        public bool TryLookupKnownTypeKey(Type type, [MaybeNullWhen(false)] out string typeKey) => _keyLookup.TryGetValue(type, out typeKey);
    }
}
