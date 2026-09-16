// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Tests.Mappers;

using Aqua.Protobuf.Mappers;
using Google.Protobuf;

internal static class MapperTestHelper
{
    public static T RoundTrip<T, TProto>(IProtoMapper<T, TProto> mapper, T value, ProtoOptions options = null)
        where TProto : IMessage
    {
        var proto = mapper.ToProto(value, ProtoContext.ForWrite(options));
        return mapper.FromProto(proto, ProtoContext.ForRead(options));
    }
}
