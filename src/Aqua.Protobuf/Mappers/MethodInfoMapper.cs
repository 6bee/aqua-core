// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Mappers;

using Aqua.TypeSystem;
using static Aqua.Protobuf.Schema.MethodInfo;
using Proto = Aqua.Protobuf.Schema;

public sealed class MethodInfoMapper : ProtoMapper<MethodInfo, Proto.MethodInfo>
{
    public static readonly MethodInfoMapper Instance = new();

    public override MethodInfo FromProto(Proto.MethodInfo proto, ProtoContext context)
    {
        return proto?.NodeCase switch
        {
            null or
            NodeOneofCase.Null => null!,
            NodeOneofCase.Value => context.Resolve<MethodInfo, Proto.MethodInfoValue>(proto.Value, FromProto),
            NodeOneofCase.Ref => context.Resolve<MethodInfo>(proto.Ref),
            _ => throw new NotSupportedException($"{proto.NodeCase} is not supported"),
        };

        static void FromProto(MethodInfo value, Proto.MethodInfoValue proto, ProtoContext context)
        {
            value.Name = proto.Name;
            value.IsStatic = proto.HasIsStatic ? proto.IsStatic : null;
            value.DeclaringType = TypeInfoMapper.Instance.FromProto(proto.DeclaringType, context);
            value.ReturnType = TypeInfoMapper.Instance.FromProto(proto.ReturnType, context);
            value.ParameterTypes = TypeInfoMapper.Instance.FromProto(proto.ParameterTypes, context).ToListOrNull();
            value.GenericArgumentTypes = TypeInfoMapper.Instance.FromProto(proto.GenericArgumentTypes, context).ToListOrNull();
        }
    }

    public override Proto.MethodInfo ToProto(MethodInfo value, ProtoContext context)
    {
        return context.ToReferenceProto<Proto.MethodInfo, Proto.MethodInfoValue, MethodInfo>(value, ToProto);

        static void ToProto(Proto.MethodInfoValue proto, MethodInfo value, ProtoContext context)
        {
            proto.Name = value.Name ?? string.Empty;

            if (value.IsStatic.HasValue)
            {
                proto.IsStatic = value.IsStatic.Value;
            }

            proto.DeclaringType = TypeInfoMapper.Instance.ToProto(value.DeclaringType!, context);
            proto.ReturnType = TypeInfoMapper.Instance.ToProto(value.ReturnType!, context);
            TypeInfoMapper.Instance.ToProto(proto.ParameterTypes, value.ParameterTypes, context);
            TypeInfoMapper.Instance.ToProto(proto.GenericArgumentTypes, value.GenericArgumentTypes, context);
        }
    }
}
