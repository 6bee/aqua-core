// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Mappers;

using Aqua.TypeSystem;
using Proto = Aqua.Protobuf.Schema;

public sealed class FieldInfoMapper : ProtoMapper<FieldInfo, Proto.FieldInfo>
{
    public static readonly FieldInfoMapper Instance = new();

    public override FieldInfo FromProto(Proto.FieldInfo proto, ProtoContext context)
        => proto is null ? null! : new()
        {
            Name = proto.Name,
            DeclaringType = TypeInfoMapper.Instance.FromProto(proto.DeclaringType, context),
            IsStatic = proto.HasIsStatic ? proto.IsStatic : null,
        };

    public override Proto.FieldInfo ToProto(FieldInfo value, ProtoContext context)
    {
        if (value is null)
        {
            return null!;
        }

        var result = new Proto.FieldInfo
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

        return result;
    }
}
