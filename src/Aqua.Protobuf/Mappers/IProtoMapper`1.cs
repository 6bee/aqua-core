// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Mappers;

public interface IProtoMapper<T>
{
    IMessage ToProto(T value, ProtoContext context);

    T FromProto(IMessage proto, ProtoContext context);
}
