// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf
{
    using global::ProtoBuf;
    using System;
    using System.Collections;
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
            ObjectArray = array
                .Cast<T>() // ensure correct element type
                .Cast<object>()
                .ToArray();
        }

        public Values(T[] array)
        {
            Array = array;
        }

        [ProtoMember(1, IsRequired = true, OverwriteList = true)]
        public T[] Array
        {
            get => ObjectArray?.Select(x => x is T t ? t : default).ToArray();
            set => ObjectArray = value?.Cast<object>().ToArray();
        }

        [ProtoIgnore]
        public override Type ElementType => typeof(T);
    }
#nullable restore
}
