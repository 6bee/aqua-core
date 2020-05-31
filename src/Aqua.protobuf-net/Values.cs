﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf
{
    using global::ProtoBuf;
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    [ProtoContract]
    public abstract class Values : Value
    {
        [ProtoIgnore]
        public override Type ValueType => ElementType.MakeArrayType();

        [ProtoIgnore]
        public object?[] ObjectArray
        {
            get => ((object[])ObjectValue).Select(x => x is NullValue ? null : x).ToArray();
            set => ObjectValue = value.Select(x => x ?? new NullValue()).ToArray();
        }

        [ProtoIgnore]
        public abstract Type ElementType { get; }

        [return: NotNullIfNotNull("sequence")]
        public static Values? Wrap(IEnumerable? sequence, Type elementType)
            => sequence is null
            ? null
            : (Values)Activator.CreateInstance(typeof(Values<>).MakeGenericType(elementType), new object?[] { sequence });
    }
}
