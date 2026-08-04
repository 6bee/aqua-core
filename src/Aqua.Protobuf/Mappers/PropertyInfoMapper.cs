// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Mappers;

using Aqua.TypeSystem;
using static Aqua.Protobuf.Schema.PropertyInfo;
using Proto = Aqua.Protobuf.Schema;

public sealed class PropertyInfoMapper : ProtoMapper<PropertyInfo, Proto.PropertyInfo>
{
    public static readonly PropertyInfoMapper Instance = new();

    public override PropertyInfo FromProto(Proto.PropertyInfo proto, ProtoContext context)
    {
        return proto?.NodeCase switch
        {
            null or
            NodeOneofCase.Null => null!,
            NodeOneofCase.Value => context.Resolve<PropertyInfo, Proto.PropertyInfoValue>(proto.Value, FromProto),
            NodeOneofCase.Ref => context.Resolve<PropertyInfo>(proto.Ref),
            _ => throw new NotSupportedException($"{proto.NodeCase} is not supported"),
        };

        static void FromProto(PropertyInfo value, Proto.PropertyInfoValue proto, ProtoContext context)
        {
            value.Name = proto.Name;
            value.IsStatic = proto.HasIsStatic ? proto.IsStatic : null;
            value.DeclaringType = TypeInfoMapper.Instance.FromProto(proto.DeclaringType, context);
            value.PropertyType = TypeInfoMapper.Instance.FromProto(proto.PropertyType, context);
        }
    }

    public override Proto.PropertyInfo ToProto(PropertyInfo value, ProtoContext context)
    {
        return context.ToReferenceProto<Proto.PropertyInfo, Proto.PropertyInfoValue, PropertyInfo>(value, ToProto);

        static void ToProto(Proto.PropertyInfoValue proto, PropertyInfo value, ProtoContext context)
        {
            if (value.Name?.Length > 0)
            {
                proto.Name = value.Name;
            }

            if (value.IsStatic.HasValue)
            {
                proto.IsStatic = value.IsStatic.Value;
            }

            proto.DeclaringType = TypeInfoMapper.Instance.ToProto(value.DeclaringType!, context);
            proto.PropertyType = TypeInfoMapper.Instance.ToProto(value.PropertyType!, context);
        }
    }
}
