// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf
{
    using global::ProtoBuf;
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

#nullable disable
    [ProtoContract]
    public sealed class Values<T> : Values
    {
        public Values()
        {
        }

        public Values(IEnumerable array)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (array is T[] typedArray)
            {
                ObjectValue = typedArray;
            }
            else
            {
                ObjectValue = array
                    .Cast<T>()
                    .ToArray();
            }
        }

        public Values(T[] array)
        {
            Array = array ?? throw new ArgumentNullException(nameof(array));
        }

        [ProtoMember(1, IsRequired = true, OverwriteList = true)]
        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Property used as serialization contract")]
        public T[] Array
        {
            get => (T[])ObjectValue;
            set => ObjectValue = value;
        }
    }
#nullable restore
}
