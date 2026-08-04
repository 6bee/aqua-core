// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Mappers;

using Aqua.TypeSystem;
using static Aqua.Protobuf.Schema.ConstructorInfo;
using Proto = Aqua.Protobuf.Schema;

public sealed class ConstructorInfoMapper : ProtoMapper<ConstructorInfo, Proto.ConstructorInfo>
{
    public static readonly ConstructorInfoMapper Instance = new();

    public override ConstructorInfo FromProto(Proto.ConstructorInfo proto, ProtoContext context)
    {
        return proto is null ? null! : proto.NodeCase switch
        {
            NodeOneofCase.Value => context.Resolve<ConstructorInfo, Proto.ConstructorInfoValue>(proto.Value, FromProto),
            NodeOneofCase.Ref => context.Resolve<ConstructorInfo>(proto.Ref),
            _ => throw new NotSupportedException($"{proto.NodeCase} is not supported"),
        };

        static void FromProto(ConstructorInfo value, Proto.ConstructorInfoValue proto, ProtoContext context)
        {
            value.Name = proto.Name;
            value.IsStatic = proto.HasIsStatic ? proto.IsStatic : null;
            value.DeclaringType = TypeInfoMapper.Instance.FromProto(proto.DeclaringType, context);
            value.ParameterTypes = TypeInfoMapper.Instance.FromProto(proto.ParameterTypes, context).ToListOrNull();
            value.GenericArgumentTypes = TypeInfoMapper.Instance.FromProto(proto.GenericArgumentTypes, context).ToListOrNull();
        }
    }

    public override Proto.ConstructorInfo ToProto(ConstructorInfo value, ProtoContext context)
    {
        return context.ToReferenceProto<Proto.ConstructorInfo, Proto.ConstructorInfoValue, ConstructorInfo>(value, ToProto);

        static void ToProto(Proto.ConstructorInfoValue proto, ConstructorInfo value, ProtoContext context)
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

            TypeInfoMapper.Instance.ToProto(proto.ParameterTypes, value.ParameterTypes, context);
            TypeInfoMapper.Instance.ToProto(proto.GenericArgumentTypes, value.GenericArgumentTypes, context);
        }
    }
}
