// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Schema;

using Google.Protobuf.WellKnownTypes;

public interface IReferenceProto<TProto>
{
    NullValue Null { get; set; }

    TProto Value { get; set; }

    Ref Ref { get; set; }
}
