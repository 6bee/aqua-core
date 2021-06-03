// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf
{
    using global::ProtoBuf;
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
            array.AssertNotNull(nameof(array));

            if (array is not Value<T?>[] typedArray)
            {
                typedArray = array
                    .Cast<object>()
                    .Select(x =>
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
            Array = items.CheckNotNull(nameof(items)).ToArray();
        }

        public NullableValues(Value<T?>[] array)
        {
            Array = array.CheckNotNull(nameof(array));
        }

        [ProtoMember(1, IsRequired = true, OverwriteList = true)]
        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Property used as serialization contract")]
        public Value<T?>[] Array
        {
            get => (Value<T?>[])ObjectValue!;
            set => ObjectValue = value;
        }

        protected override IEnumerable GetEnumerable()
            => Array
            .Select(x => x.TypedValue)
            .ToArray();
    }
}