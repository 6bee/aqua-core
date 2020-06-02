// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf
{
    using global::ProtoBuf;
    using System;
    using System.Diagnostics.CodeAnalysis;

    [ProtoContract]
    public sealed class Value<T> : Value
    {
        public Value()
        {
        }

        public Value(T value)
        {
            TypedValue = value;
        }

        [ProtoMember(1, IsRequired = true)]
        [NotNull]
        public T TypedValue
        {
            get => (T)ObjectValue;
            set => ObjectValue = value!;
        }
    }
}
