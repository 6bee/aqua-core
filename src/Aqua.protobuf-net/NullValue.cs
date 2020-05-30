// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf
{
    using global::ProtoBuf;
    using System;

    [ProtoContract]
    public sealed class NullValue : Value
    {
        [ProtoIgnore]
        public override object ObjectValue
        {
            get => null!;
            set => throw new InvalidOperationException("Read-only property may not be set.");
        }

        [ProtoIgnore]
        public override Type ValueType => typeof(object);
    }
}
