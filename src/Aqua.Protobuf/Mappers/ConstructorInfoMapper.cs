// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Mappers;

using Aqua.TypeSystem;
using Proto = Aqua.Protobuf.Schema;

public sealed class ConstructorInfoMapper : ProtoMapper<ConstructorInfo, Proto.ConstructorInfo>
{
    public static readonly ConstructorInfoMapper Instance = new();

    public override ConstructorInfo FromProto(Proto.ConstructorInfo proto, ProtoContext context)
        => proto is null ? null! : new()
        {
            Name = proto.Name,
            IsStatic = proto.HasIsStatic ? proto.IsStatic : null,
            DeclaringType = TypeInfoMapper.Instance.FromProto(proto.DeclaringType, context),
            ParameterTypes = TypeInfoMapper.Instance.FromProto(proto.ParameterTypes, context).ToListOrNull(),
            GenericArgumentTypes = TypeInfoMapper.Instance.FromProto(proto.GenericArgumentTypes, context).ToListOrNull(),
        };

    public override Proto.ConstructorInfo ToProto(ConstructorInfo value, ProtoContext context)
    {
        if (value is null)
        {
            return null!;
        }

        var result = new Proto.ConstructorInfo
        {
            DeclaringType = TypeInfoMapper.Instance.ToProto(value.DeclaringType!, context),
        };

        if (value.Name?.Length > 0)
        {
            result.Name = value.Name;
        }

        if (value.IsStatic.HasValue)
        {
            result.IsStatic = value.IsStatic.Value;
        }

        TypeInfoMapper.Instance.ToProto(result.GenericArgumentTypes, value.GenericArgumentTypes, context);
        TypeInfoMapper.Instance.ToProto(result.ParameterTypes, value.ParameterTypes, context);

        return result;
    }
}
