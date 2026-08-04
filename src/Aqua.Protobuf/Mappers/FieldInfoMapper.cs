// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Mappers;

using Aqua.TypeSystem;
using static Aqua.Protobuf.Schema.FieldInfo;
using Proto = Aqua.Protobuf.Schema;

public sealed class FieldInfoMapper : ProtoMapper<FieldInfo, Proto.FieldInfo>
{
    public static readonly FieldInfoMapper Instance = new();

    public override FieldInfo FromProto(Proto.FieldInfo proto, ProtoContext context)
    {
        return proto?.NodeCase switch
        {
            null or
            NodeOneofCase.Null => null!,
            NodeOneofCase.Value => context.Resolve<FieldInfo, Proto.FieldInfoValue>(proto.Value, FromProto),
            NodeOneofCase.Ref => context.Resolve<FieldInfo>(proto.Ref),
            _ => throw new NotSupportedException($"{proto.NodeCase} is not supported"),
        };

        static void FromProto(FieldInfo value, Proto.FieldInfoValue proto, ProtoContext context)
        {
            value.Name = proto.Name;
            value.IsStatic = proto.HasIsStatic ? proto.IsStatic : null;
            value.DeclaringType = TypeInfoMapper.Instance.FromProto(proto.DeclaringType, context);
        }
    }

    public override Proto.FieldInfo ToProto(FieldInfo value, ProtoContext context)
    {
        return context.ToReferenceProto<Proto.FieldInfo, Proto.FieldInfoValue, FieldInfo>(value, ToProto);

        static void ToProto(Proto.FieldInfoValue proto, FieldInfo value, ProtoContext context)
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
        }
    }
}
