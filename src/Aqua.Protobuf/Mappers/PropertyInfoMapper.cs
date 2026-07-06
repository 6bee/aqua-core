// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Mappers;

using Aqua.TypeSystem;
using Proto = Aqua.Protobuf.Schema;

public sealed class PropertyInfoMapper : ProtoMapper<PropertyInfo, Proto.PropertyInfo>
{
    public static readonly PropertyInfoMapper Instance = new();

    public override PropertyInfo FromProto(Proto.PropertyInfo proto, ProtoContext context)
        => proto is null ? null! : new()
        {
            Name = proto.Name,
            PropertyType = TypeInfoMapper.Instance.FromProto(proto.PropertyType, context),
            DeclaringType = TypeInfoMapper.Instance.FromProto(proto.DeclaringType, context),
            IsStatic = proto.HasIsStatic ? proto.IsStatic : null,
        };

    public override Proto.PropertyInfo ToProto(PropertyInfo value, ProtoContext context)
    {
        if (value is null)
        {
            return null!;
        }

        var result = new Proto.PropertyInfo
        {
            DeclaringType = TypeInfoMapper.Instance.ToProto(value.DeclaringType!, context),
            PropertyType = TypeInfoMapper.Instance.ToProto(value.PropertyType!, context),
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
