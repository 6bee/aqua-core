// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Mappers;

using Proto = Aqua.Protobuf.Schema;

/// <summary>
/// Allows to register <see cref="IProtoMapper{T, TProto}"/>
/// for CLR types handled by <see cref="ValueMapper"/> as <see cref="ValueMapper"/>
/// itself is registered for <see cref="object"/> type only.
/// </summary>
/// <remarks>
/// <see cref="ValueMapper{T}"/> is specifically used for mapping primitive values (value types)
/// which are registered both, as nullable as well as non-nullable types.
/// </remarks>
internal sealed class ValueMapper<T> : ProtoMapper<T, Proto.Value>
{
    public static readonly ValueMapper<T> Instance = new();

    public override T FromProto(Proto.Value proto, ProtoContext context)
        => (T)ValueMapper.Instance.FromProto(proto, context)!;

    public override Proto.Value ToProto(T value, ProtoContext context)
        => ValueMapper.Instance.ToProto(value, context);
}
