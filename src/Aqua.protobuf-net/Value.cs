// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf
{
    using Aqua.EnumerableExtensions;
    using Aqua.ProtoBuf.Dynamic;
    using Aqua.TypeSystem;
    using global::ProtoBuf;
    using System;
    using System.Diagnostics.CodeAnalysis;

    [ProtoContract]
    [ProtoInclude(1, typeof(Values))]
    [ProtoInclude(2, typeof(DynamicObjectSurrogate))]
    [ProtoInclude(3, typeof(DynamicObjectArraySurrogate))]
    public abstract class Value
    {
        [ProtoIgnore]
        public virtual object ObjectValue { get; set; } = null!;

        [return: NotNullIfNotNull("value")]
        public static Value? Wrap(object? value) => Wrap(value, null);

        [return: NotNullIfNotNull("value")]
        internal static Value? Wrap(object? value, Type? type)
            => value is null
            ? null
            : value is Value v
            ? v
            : value.IsCollection(out var collection)
            ? Values.Wrap(collection, TypeHelper.GetElementType(type) ?? TypeHelper.GetElementType(collection.GetType()) !)
            : (Value)Activator.CreateInstance(typeof(Value<>).MakeGenericType(value.GetType()), new object[] { value });
    }
}
