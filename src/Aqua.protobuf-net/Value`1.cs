// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf
{
    using global::ProtoBuf;
    using System;

    [ProtoContract]
    public sealed class Value<T> : Value
        where T : notnull
    {
        public Value()
        {
        }

        public Value(T value)
        {
            TypedValue = value;
        }

        [ProtoMember(1, IsRequired = true)]
        public T TypedValue
        {
            get => (T)ObjectValue;
            set => ObjectValue = value;
        }

        [ProtoIgnore]
        public override Type ValueType => typeof(T);
    }
}
