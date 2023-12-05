// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf.Dynamic;

using Aqua.Dynamic;
using Aqua.EnumerableExtensions;
using Aqua.TypeExtensions;
using Aqua.TypeSystem;
using global::ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

[ProtoContract(Name = nameof(DynamicObject))]
public sealed class DynamicObjectSurrogate
{
    [ProtoMember(1)]
    public TypeInfo? Type { get; set; }

    [ProtoMember(2, OverwriteList = true)]
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Required for serialization")]
    public Dictionary<string, Value?>? Properties { get; set; }

    [ProtoMember(3)]
    public bool? IsNull { get; set; }

    [ProtoConverter]
    public static DynamicObjectSurrogate Convert(DynamicObject? source)
        => source is null
        ? new DynamicObjectSurrogate()
        : new DynamicObjectSurrogate
        {
            Type = source.Type,
            IsNull = source.Properties?.Count > 0 ? null : source.IsNull,
            Properties = Convert(source.Properties, source.Type),
        };

    private static Dictionary<string, Value?>? Convert(PropertySet? properties, TypeInfo? dynamicObjectType)
    {
        if (properties is null)
        {
            return null!;
        }

        var wrappedType = default(Type);
        if (properties.Count is 1 && string.IsNullOrEmpty(properties.Single().Name))
        {
            wrappedType = dynamicObjectType?.ToType();
        }

        var map = properties.ToDictionary<Property, string, Value?>(
            static x => x.Name,
            x => Wrap(x.Value, wrappedType));
        return map;
    }

    [ProtoConverter]
    [return: NotNullIfNotNull(nameof(surrogate))]
    public static DynamicObject? Convert(DynamicObjectSurrogate? surrogate)
        => surrogate is null || (surrogate.Type is null && surrogate.Properties is null)
        ? null
        : surrogate.Properties is null && surrogate.IsNull is true
        ? DynamicObject.CreateDefault(surrogate.Type)
        : new DynamicObject(
            surrogate.Type,
            Convert(surrogate.Properties));

    private static PropertySet? Convert(Dictionary<string, Value?>? properties)
    {
        if (properties is null)
        {
            return null!;
        }

        var items = properties.ToDictionary(
            static x => x.Key,
            static x => Value.Unwrap(x.Value));

        return PropertySet.From(items);
    }

    private static Value Wrap(object? value, Type? wrappedType)
    {
        if (value.IsCollection(out var collection))
        {
            if (!collection.Any())
            {
                return EmptyArray.Instance;
            }

            IEnumerable? TryCast(Type elementType, bool needAny = true)
            {
                if (elementType == typeof(object))
                {
                    return null!;
                }

                var isNullableType = elementType.IsNullableType();
                return (!needAny || collection.Any(elementType.IsInstanceOfType))
                    && collection.All(x => elementType.IsInstanceOfType(x) || (x is null && isNullableType))
                    ? collection.CastCollectionToArrayOfType(elementType)
                    : null!;
            }

            var elementType = typeof(object);
            if (wrappedType is not null)
            {
                elementType = TypeHelper.GetElementType(wrappedType)
                    ?? throw new InvalidOperationException($"Failed to get element type of {wrappedType}");
            }

            var array =
                TryCast(elementType, false) ??
                TryCast(typeof(DynamicObject)) ??
                TryCast(typeof(string)) ??
                collection;
            value = array;
        }

        return Value.Wrap(value) ?? NullValue.Instance;
    }
}