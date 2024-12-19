// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf;

using Aqua.Dynamic;
using Aqua.EnumerableExtensions;
using Aqua.ProtoBuf.Dynamic;
using Aqua.TypeSystem;
using global::ProtoBuf;
using System;
using System.Diagnostics.CodeAnalysis;

[ProtoContract]
[ProtoInclude(1, typeof(Values))]
[ProtoInclude(2, typeof(NullValue))]
[ProtoInclude(3, typeof(EmptyArray))]
public abstract class Value
{
    [ProtoIgnore]
    public virtual object? ObjectValue { get; protected set; }

    [return: NotNullIfNotNull(nameof(value))]
    public static Value? Wrap(object? value) => Wrap(value, null);

    [return: NotNullIfNotNull(nameof(value))]
    internal static Value? Wrap(object? value, Type? type)
        => value is null
        ? null
        : value is Value v
        ? v
        : value is DynamicObject dynamicObject
        ? new Value<DynamicObjectSurrogate>(DynamicObjectSurrogate.Convert(dynamicObject))
        : value.IsCollection(out var collection)
        ? Values.Wrap(collection!, TypeHelper.GetElementType(type) ?? TypeHelper.GetElementType(collection.GetType()))
        : (Value?)Activator.CreateInstance(typeof(Value<>).MakeGenericType(value.GetType()), value);

    [return: NotNullIfNotNull(nameof(value))]
    public static object? Unwrap(Value? value)
        => value is null or NullValue
        ? null
        : value is EmptyArray
        ? Array.Empty<object>()
        : value is Value<DynamicObjectSurrogate> dynamicObject
        ? DynamicObjectSurrogate.Convert(dynamicObject.TypedValue)
        : value is Values values
        ? Values.Unwrap(values)
        : value.ObjectValue;
}