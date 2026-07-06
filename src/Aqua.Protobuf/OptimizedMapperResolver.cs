// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf;

using Aqua.Protobuf.Mappers;

internal sealed class OptimizedMapperResolver(IMapperResolver resolver) : IMapperResolver
{
    public IProtoMapper<T>? GetMapper<T>()
    {
        if (Cache<T>.Mapper is not { } mapper)
        {
#pragma warning disable S2696 // Instance members should not write to "static" fields
            Cache<T>.Mapper = mapper = resolver.GetMapper<T>();
#pragma warning restore S2696 // Instance members should not write to "static" fields
        }

        return mapper;
    }

    private static class Cache<T>
    {
#pragma warning disable S2223 // Non-constant static fields should not be visible
#pragma warning disable SA1401 // Fields should be private
        public static IProtoMapper<T>? Mapper;
#pragma warning restore SA1401 // Fields should be private
#pragma warning restore S2223 // Non-constant static fields should not be visible
    }
}
