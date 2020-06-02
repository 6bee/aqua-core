// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf
{
    using global::ProtoBuf;
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;

    [ProtoContract]
    public abstract class Values : Value
    {
        [return: NotNullIfNotNull("sequence")]
        public static Values? Wrap(IEnumerable? sequence, Type elementType)
            => sequence is null
            ? null
            : (Values)Activator.CreateInstance(typeof(Values<>).MakeGenericType(elementType), new object?[] { sequence });
    }
}
