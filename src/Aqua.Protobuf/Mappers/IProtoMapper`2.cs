// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Mappers;

public interface IProtoMapper<T, TProto> : IProtoMapper<T>
    where TProto : IMessage?
{
    new TProto ToProto(T value, ProtoContext context);

    T FromProto(TProto proto, ProtoContext context);
}
