// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf
{
    using global::ProtoBuf;
    using System;
    using System.Diagnostics.CodeAnalysis;

    [ProtoContract]
    public abstract class Value
    {
        [ProtoIgnore]
        public object ObjectValue { get; set; } = null!;

        [ProtoIgnore]
        public abstract Type Type { get; }

        [return: NotNullIfNotNull("value")]
        public static Value? Wrap(object? value)
            => value is null
            ? null
            : value is Value v
            ? v
            : (Value)Activator.CreateInstance(typeof(Value<>).MakeGenericType(value.GetType()), new object[] { value });
    }
}
