// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf.Dynamic
{
    using Aqua.Dynamic;
    using Aqua.Extensions;
    using Aqua.TypeSystem;
    using global::ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [ProtoContract(Name = nameof(DynamicObject))]
    public class DynamicObjectSurrogate
    {
        [ProtoMember(1)]
        public TypeInfo? Type { get; set; }

        [ProtoMember(2, DynamicType = true)]
        public Dictionary<string, object?> Properties { get; set; } = null!;

        [ProtoConverter]
        public static DynamicObjectSurrogate? Convert(DynamicObject? source)
            => source is null
            ? null
            : new DynamicObjectSurrogate
                {
                    Type = source.Type,
                    Properties = Map(source.Properties, source.Type?.Type, source.IsSingleValueWrapper()),
                };

        [ProtoConverter]
        public static DynamicObject? Convert(DynamicObjectSurrogate? surrogate)
            => surrogate is null
            ? null
            : new DynamicObject(surrogate.Type, Unwrap(surrogate.Properties));

        private static PropertySet? Unwrap(Dictionary<string, object?> properties)
            => properties.Count == 1 && properties.Keys.Single().Length == 0
            ? new PropertySet
                {
                    { string.Empty, UnwrapSingle(properties.Values.Single()) },
                }
            : new PropertySet(properties);

        private static object? UnwrapSingle(object? element)
            => element is Values values
            ? values.ObjectArray
            : element is Value value
            ? value.ObjectValue
            : element;

        private static Dictionary<string, object?> Map(PropertySet properties, Type? type, bool singleValue)
            => singleValue
            ? properties.ToDictionary(x => x.Name, x => MapSingle(x.Value, type))
            : properties.ToDictionary(x => x.Name, x => x.Value);

        private static object? MapSingle(object? value, Type? type = null)
        {
            if (value is null || type is null)
            {
                return value;
            }

            if (value.IsCollection(out var collection))
            {
                return Values.Wrap(collection, TypeHelper.GetElementType(type) ?? type);
            }

            return Value.Wrap(value);
        }
    }
}
