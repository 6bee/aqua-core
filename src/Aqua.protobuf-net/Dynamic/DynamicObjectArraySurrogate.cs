// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf.Dynamic
{
    using Aqua.Dynamic;
    using global::ProtoBuf;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    [ProtoContract(Name = "DynamicObjectCollection")]
    public class DynamicObjectArraySurrogate : Value
    {
        [ProtoIgnore]
        public override object ObjectValue
        {
            get => Convert(this);
            set => throw new InvalidOperationException("Read-only property may not be set.");
        }

        [ProtoMember(1, IsRequired = true, OverwriteList = true)]
        public DynamicObjectSurrogate?[] Collection { get; set; } = null!;

        [return: NotNullIfNotNull("source")]
#pragma warning disable SA1011 // Closing square brackets should be spaced correctly
        public static DynamicObjectArraySurrogate? Convert(DynamicObject?[]? source)
#pragma warning restore SA1011 // Closing square brackets should be spaced correctly
            => source is null
            ? null
            : new DynamicObjectArraySurrogate
            {
                Collection = source.Select(DynamicObjectSurrogate.Convert).ToArray(),
            };

        [return: NotNullIfNotNull("surrogate")]
#pragma warning disable SA1011 // Closing square brackets should be spaced correctly
        public static DynamicObject?[]? Convert(DynamicObjectArraySurrogate? surrogate)
#pragma warning restore SA1011 // Closing square brackets should be spaced correctly
            => surrogate is null
            ? null
            : surrogate.Collection.Select(DynamicObjectSurrogate.Convert).ToArray();
    }
}
