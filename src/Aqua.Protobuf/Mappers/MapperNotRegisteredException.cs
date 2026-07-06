// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Mappers;

using System.Runtime.Serialization;

[Serializable]
public class MapperNotRegisteredException : ProtobufSerializationException
{
    public MapperNotRegisteredException(string? message)
        : base(message)
    {
    }

#if NETSTANDARD2_0
    protected MapperNotRegisteredException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
#endif // NETSTANDARD2_0
}
