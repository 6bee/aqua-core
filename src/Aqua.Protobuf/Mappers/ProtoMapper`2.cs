// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Mappers;

public abstract class ProtoMapper<T, TProto> : IProtoMapper<T, TProto>
    where TProto : IMessage?
{
    public abstract T FromProto(TProto proto, ProtoContext context);

    public abstract TProto ToProto(T value, ProtoContext context);

    T IProtoMapper<T>.FromProto(IMessage proto, ProtoContext context) => FromProto((TProto)proto, context);

    IMessage IProtoMapper<T>.ToProto(T value, ProtoContext context) => ToProto(value, context)!;
}
