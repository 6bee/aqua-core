// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf;

using global::ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

/// <summary>
/// Collection wrapper allowing element values to be <see langword="null"/>.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
[ProtoContract]
public sealed class NullableValues<T> : Values
{
    public NullableValues()
    {
    }

    public NullableValues(IEnumerable array)
    {
        array.AssertNotNull();

        if (array is not Value<T?>[] typedArray)
        {
            typedArray = array
                .Cast<object>()
                .Select(static x =>
                {
                    if (x is Value<T?> v)
                    {
                        return v;
                    }

                    if (x is T t)
                    {
                        return new Value<T?>(t);
                    }

                    return new Value<T?>(default);
                })
                .ToArray();
        }

        Array = typedArray;
    }

    public NullableValues(IEnumerable<Value<T?>> items)
    {
        Array = items.CheckNotNull().ToArray();
    }

    public NullableValues(Value<T?>[] array)
    {
        Array = array.CheckNotNull();
    }

    [ProtoIgnore]
    public override object? ObjectValue
    {
        get => Array.Select(static x => x.TypedValue).ToArray();
        protected set => throw new NotSupportedException($"Items must be set via {nameof(Array)} property");
    }

    [ProtoMember(1, IsRequired = true, OverwriteList = true)]
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Property used as serialization contract")]
    public Value<T?>[] Array
    {
        get => MapInternal((object[])base.ObjectValue!);
        set => base.ObjectValue = MapInternal(value);
    }

    private static Value<T?>[] MapInternal(object[] array)
        => (array ?? System.Array.Empty<object>())
        .Select(static x => x is null or NullValue ? new Value<T?>(default) : (Value<T?>)x)
        .ToArray();

    private static object[] MapInternal(Value<T?>[] array)
        => array
        .Select(static x => x?.ObjectValue is null ? NullValue.Instance : (Value)x)
        .ToArray();

    protected override IEnumerable GetEnumerable()
        => Array
        .Select(static x => x.TypedValue)
        .ToArray();
}