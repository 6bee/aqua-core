// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf
{
    using global::ProtoBuf;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    [ProtoContract]
    public sealed class Values<T> : Values
    {
        public Values()
        {
        }

        public Values(IEnumerable array)
        {
            array.AssertNotNull(nameof(array));

            if (array is not T[] typedArray)
            {
                typedArray = array
                    .Cast<T>()
                    .ToArray();
            }

            Array = typedArray;
        }

        public Values(T[] array)
        {
            Array = array.CheckNotNull(nameof(array));
        }

        [ProtoMember(1, IsRequired = true, OverwriteList = true)]
        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Property used as serialization contract")]
        public T[] Array
        {
            get => (T[])ObjectValue!;
            set => ObjectValue = value;
        }

        protected override IEnumerable GetEnumerable()
            => Array;
    }
}