// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf;

using Aqua.Protobuf.Mappers;
using Aqua.TypeExtensions;
using System.ComponentModel;
using System.Numerics;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class MapperResolverExtensions
{
    extension(MapperResolver resolver)
    {
        internal static MapperResolver Empty => new();

        public MapperResolver AddAquaTypes()
        {
            resolver.AddMapper(ValueMapper.Instance);

            resolver.AddMapper(ValueMapper<string>.Instance);
            resolver.AddMapper(ValueMapper<Array>.Instance);
            resolver.AddMapper(ValueMapper<bool>.Instance);
            resolver.AddMapper(ValueMapper<bool?>.Instance);
            resolver.AddMapper(ValueMapper<byte>.Instance);
            resolver.AddMapper(ValueMapper<byte?>.Instance);
            resolver.AddMapper(ValueMapper<sbyte>.Instance);
            resolver.AddMapper(ValueMapper<sbyte?>.Instance);
            resolver.AddMapper(ValueMapper<short>.Instance);
            resolver.AddMapper(ValueMapper<short?>.Instance);
            resolver.AddMapper(ValueMapper<ushort>.Instance);
            resolver.AddMapper(ValueMapper<ushort?>.Instance);
            resolver.AddMapper(ValueMapper<int>.Instance);
            resolver.AddMapper(ValueMapper<int?>.Instance);
            resolver.AddMapper(ValueMapper<uint>.Instance);
            resolver.AddMapper(ValueMapper<uint?>.Instance);
            resolver.AddMapper(ValueMapper<long>.Instance);
            resolver.AddMapper(ValueMapper<long?>.Instance);
            resolver.AddMapper(ValueMapper<ulong>.Instance);
            resolver.AddMapper(ValueMapper<ulong?>.Instance);
            resolver.AddMapper(ValueMapper<float>.Instance);
            resolver.AddMapper(ValueMapper<float?>.Instance);
            resolver.AddMapper(ValueMapper<double>.Instance);
            resolver.AddMapper(ValueMapper<double?>.Instance);
            resolver.AddMapper(ValueMapper<decimal>.Instance);
            resolver.AddMapper(ValueMapper<decimal?>.Instance);
            resolver.AddMapper(ValueMapper<Guid>.Instance);
            resolver.AddMapper(ValueMapper<Guid?>.Instance);
            resolver.AddMapper(ValueMapper<DateTime>.Instance);
            resolver.AddMapper(ValueMapper<DateTime?>.Instance);
            resolver.AddMapper(ValueMapper<DateTimeOffset>.Instance);
            resolver.AddMapper(ValueMapper<DateTimeOffset?>.Instance);
            resolver.AddMapper(ValueMapper<TimeSpan>.Instance);
            resolver.AddMapper(ValueMapper<TimeSpan?>.Instance);
            resolver.AddMapper(ValueMapper<BigInteger>.Instance);
            resolver.AddMapper(ValueMapper<BigInteger?>.Instance);
            resolver.AddMapper(ValueMapper<Complex>.Instance);
            resolver.AddMapper(ValueMapper<Complex?>.Instance);
#if NET5_0_OR_GREATER
            resolver.AddMapper(ValueMapper<Half>.Instance);
            resolver.AddMapper(ValueMapper<Half?>.Instance);
#endif // NET5_0_OR_GREATER
#if NET6_0_OR_GREATER
            resolver.AddMapper(ValueMapper<TimeOnly>.Instance);
            resolver.AddMapper(ValueMapper<TimeOnly?>.Instance);
            resolver.AddMapper(ValueMapper<DateOnly>.Instance);
            resolver.AddMapper(ValueMapper<DateOnly?>.Instance);
#endif // NET6_0_OR_GREATER
#if NET7_0_OR_GREATER
            resolver.AddMapper(ValueMapper<Int128>.Instance);
            resolver.AddMapper(ValueMapper<Int128?>.Instance);
            resolver.AddMapper(ValueMapper<UInt128>.Instance);
            resolver.AddMapper(ValueMapper<UInt128?>.Instance);
#endif // NET7_0_OR_GREATER

            resolver.AddMapper(DynamicObjectMapper.Instance);

            resolver.AddMapper(TypeInfoMapper.Instance);
            resolver.AddMapper(MemberInfoMapper.Instance);
            resolver.AddMapper(PropertyInfoMapper.Instance);
            resolver.AddMapper(FieldInfoMapper.Instance);
            resolver.AddMapper(MethodInfoMapper.Instance);
            resolver.AddMapper(ConstructorInfoMapper.Instance);

            return resolver;
        }

        public IMapperResolver AddMapper<T, TProto>(IProtoMapper<T, TProto> mapper)
            where TProto : IMessage
            => resolver.TryAddMapper(mapper)
            ? resolver
            : throw new InvalidOperationException($"Mapper for type {typeof(T).GetFriendlyName()} has already been registered.");
    }

    extension(IMapperResolver resolver)
    {
        public IProtoMapper<T> GetMapperWithVerify<T>()
        {
            resolver.AssertNotNull();

            return resolver.GetMapper<T>()
                ?? throw new MapperNotRegisteredException($"{typeof(T).GetFriendlyName()} is not registered in resolver: {resolver.GetType().GetFriendlyName()}");
        }

        /// <summary>
        /// Creates a performance optimized resolver, that uses shared static state
        /// to cache resolved instances for fast lookup.
        /// </summary>
        public IMapperResolver Optimized()
        {
            resolver.AssertNotNull();
            return resolver as OptimizedMapperResolver ?? new OptimizedMapperResolver(resolver);
        }
    }
}
