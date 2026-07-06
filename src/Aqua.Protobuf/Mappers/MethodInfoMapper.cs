// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Mappers;

using Aqua.TypeSystem;
using Proto = Aqua.Protobuf.Schema;

public sealed class MethodInfoMapper : ProtoMapper<MethodInfo, Proto.MethodInfo>
{
    public static readonly MethodInfoMapper Instance = new();

    public override MethodInfo FromProto(Proto.MethodInfo proto, ProtoContext context)
        => proto is null ? null! : new()
        {
            Name = proto.Name,
            DeclaringType = TypeInfoMapper.Instance.FromProto(proto.DeclaringType, context),
            IsStatic = proto.HasIsStatic ? proto.IsStatic : null,
            ReturnType = TypeInfoMapper.Instance.FromProto(proto.ReturnType, context),
            ParameterTypes = TypeInfoMapper.Instance.FromProto(proto.ParameterTypes, context).ToListOrNull(),
            GenericArgumentTypes = TypeInfoMapper.Instance.FromProto(proto.GenericArgumentTypes, context).ToListOrNull(),
        };

    public override Proto.MethodInfo ToProto(MethodInfo value, ProtoContext context)
    {
        if (value is null)
        {
            return null!;
        }

        var result = new Proto.MethodInfo
        {
            Name = value.Name ?? string.Empty,
            DeclaringType = TypeInfoMapper.Instance.ToProto(value.DeclaringType!, context),
            ReturnType = TypeInfoMapper.Instance.ToProto(value.ReturnType!, context),
        };

        if (value.IsStatic.HasValue)
        {
            result.IsStatic = value.IsStatic.Value;
        }

        TypeInfoMapper.Instance.ToProto(result.GenericArgumentTypes, value.GenericArgumentTypes, context);
        TypeInfoMapper.Instance.ToProto(result.ParameterTypes, value.ParameterTypes, context);

        return result;
    }
}
