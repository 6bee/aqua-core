// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf
{
    using Aqua.Dynamic;
    using Aqua.ProtoBuf.Dynamic;
    using Aqua.TypeExtensions;
    using global::ProtoBuf;
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    [ProtoContract]
    public abstract class Values : Value
    {
        [return: NotNullIfNotNull("sequence")]
        public static Values? Wrap(IEnumerable? sequence, Type elementType)
            => sequence is null
            ? null
            : sequence is DynamicObject[] dynamicObjectArray
            ? new Values<DynamicObjectSurrogate>(dynamicObjectArray.Select(DynamicObjectSurrogate.Convert))
            : elementType.IsNullableType() && sequence.Cast<object>().Any(x => x is null)
            ? (Values?)Activator.CreateInstance(typeof(NullableValues<>).MakeGenericType(elementType), new object[] { sequence })
            : (Values?)Activator.CreateInstance(typeof(Values<>).MakeGenericType(elementType), new object[] { sequence });

        [return: NotNullIfNotNull("values")]
        public static IEnumerable? Unwrap(Values? values)
            => values is Values<DynamicObjectSurrogate> dynamicObjects
            ? dynamicObjects.Array.Select(DynamicObjectSurrogate.Convert).ToArray()
            : values?.GetEnumerable();

        protected abstract IEnumerable GetEnumerable();
    }
}