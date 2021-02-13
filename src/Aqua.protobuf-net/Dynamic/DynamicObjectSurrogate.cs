// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf.Dynamic
{
    using Aqua.Dynamic;
    using Aqua.EnumerableExtensions;
    using Aqua.TypeSystem;
    using global::ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    [ProtoContract(Name = nameof(DynamicObject))]
    public class DynamicObjectSurrogate : Value
    {
        [ProtoIgnore]
        public override object ObjectValue
        {
            get => Convert(this);
            set => throw new InvalidOperationException("Read-only property may not be set.");
        }

        [ProtoMember(1)]
        public TypeInfo? Type { get; set; }

        [ProtoMember(2, OverwriteList = true)]
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Required for serialization")]
        public Dictionary<string, Value?>? Properties { get; set; }

        [ProtoConverter]
        [return: NotNullIfNotNull("source")]
        public static DynamicObjectSurrogate? Convert(DynamicObject? source)
            => source is null
            ? null
            : new DynamicObjectSurrogate
            {
                Type = source.Type,
                Properties = Map(source),
            };

        [ProtoConverter]
        [return: NotNullIfNotNull("surrogate")]
        public static DynamicObject? Convert(DynamicObjectSurrogate? surrogate)
            => surrogate is null
            ? null
            : surrogate.Properties is null
            ? new DynamicObject(surrogate.Type) { IsNull = true }
            : new DynamicObject(surrogate.Type, Unwrap(surrogate.Properties!));

        private static PropertySet Unwrap(Dictionary<string, Value?> properties)
            => new PropertySet(properties.Select(x => new Property(x.Key, x.Value?.ObjectValue)));

        private static Dictionary<string, Value?>? Map(DynamicObject source)
            => source.Properties?.ToDictionary(
                x => x.Name,
                x => WrapValue(x.Value, source.IsSingleValueWrapper() ? source.Type : null));

        private static Value? WrapValue(object? value, TypeInfo? type)
            => value is DynamicObject dynamicObject
            ? DynamicObjectSurrogate.Convert(dynamicObject)
            : value is DynamicObject?[] dynamicObjectArray
                ? DynamicObjectArraySurrogate.Convert(dynamicObjectArray)
                : value is object?[] objectArray && objectArray.All(x => x is DynamicObject)
                    ? DynamicObjectArraySurrogate.Convert(objectArray.Cast<DynamicObject?>().ToArray())
                    : Value.Wrap(value, RedirectTypeForStringFormattedValues(value, type?.ToType()));

        private static Type? RedirectTypeForStringFormattedValues(object? value, Type? type)
        {
            if (type is not null &&
                value.IsCollection(out var collection) &&
                collection.Any(x => x is not null) &&
                collection.All(x => x is null || x is string))
            {
                return typeof(string);
            }

            return type;
        }
    }
}