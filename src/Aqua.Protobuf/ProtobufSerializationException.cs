// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf;

[Serializable]
public class ProtobufSerializationException : Exception
{
    public ProtobufSerializationException(string? message)
        : base(message)
    {
    }

#if NETSTANDARD2_0
    protected ProtobufSerializationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
    {
    }
#endif // NETSTANDARD2_0
}
