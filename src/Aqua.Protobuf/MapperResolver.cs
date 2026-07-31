// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf;

using Aqua.Protobuf.Mappers;
using System.Diagnostics;

public sealed class MapperResolver : IMapperResolver
{
    private readonly Dictionary<Type, object> _mapper = [];

    [StackTraceHidden]
    public bool TryAddMapper<T, TProto>(IProtoMapper<T, TProto> mapper)
        where TProto : IMessage
    {
        mapper.AssertNotNull();
        return _mapper.TryAdd(typeof(T), mapper);
    }

    public IProtoMapper<T>? GetMapper<T>()
    {
        if (_mapper.TryGetValue(typeof(T), out var resolver))
        {
            return (IProtoMapper<T>)resolver;
        }

        return null;
    }
}

#if NETSTANDARD2_0
file static class Pilyfill
{
    extension<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
    {
        public bool TryAdd(TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                return false;
            }

            dictionary.Add(key, value);
            return true;
        }
    }
}
#endif // NETSTANDARD2_0
