// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf;

using Aqua.Protobuf.Mappers;

public interface IMapperResolver
{
    IProtoMapper<T>? GetMapper<T>();
}
