// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Mappers;

using Aqua.TypeSystem;
using static Aqua.Protobuf.Schema.TypeInfo;
using Proto = Aqua.Protobuf.Schema;

public sealed class TypeInfoMapper : ProtoMapper<TypeInfo, Proto.TypeInfo>
{
    public static readonly TypeInfoMapper Instance = new();

    public override TypeInfo FromProto(Proto.TypeInfo proto, ProtoContext context)
    {
        return proto?.NodeCase switch
        {
            null or
            NodeOneofCase.Null => null!,
            NodeOneofCase.Value => context.Resolve<TypeInfo, Proto.TypeInfoValue>(proto.Value, FromProto),
            NodeOneofCase.Ref => context.Resolve<TypeInfo>(proto.Ref),
            _ => throw new NotSupportedException($"{proto.NodeCase} is not supported"),
        };

        static void FromProto(TypeInfo value, Proto.TypeInfoValue proto, ProtoContext context)
        {
            value.Name = proto.Name.Length == 0 ? null : proto.Name;
            value.Namespace = proto.Namespace.Length == 0 ? null : proto.Namespace;
            value.IsAnonymousType = proto.IsAnonymousType;
            value.IsGenericType = proto.IsGenericType;
            value.DeclaringType = TypeInfoMapper.Instance.FromProto(proto.DeclaringType, context);
            value.GenericArguments = TypeInfoMapper.Instance.FromProto(proto.GenericArguments, context).ToListOrNull();
            value.Properties = PropertyInfoMapper.Instance.FromProto(proto.Properties, context).ToListOrNull();
        }
    }

    public override Proto.TypeInfo ToProto(TypeInfo value, ProtoContext context)
    {
        if (value is null)
        {
            return new() { Null = Google.Protobuf.WellKnownTypes.NullValue.NullValue };
        }

        return context.ToReferenceProto<Proto.TypeInfo, Proto.TypeInfoValue, TypeInfo>(value, ToProto);

        static void ToProto(Proto.TypeInfoValue proto, TypeInfo value, ProtoContext context)
        {
            if (value.Name?.Length > 0)
            {
                proto.Name = value.Name;
            }

            if (value.Namespace?.Length > 0)
            {
                proto.Namespace = value.Namespace;
            }

            proto.IsAnonymousType = value.IsAnonymousType;
            proto.IsGenericType = value.IsGenericType;

            proto.DeclaringType = TypeInfoMapper.Instance.ToProto(value.DeclaringType!, context);
            TypeInfoMapper.Instance.ToProto(proto.GenericArguments, value.GenericArguments, context);
            PropertyInfoMapper.Instance.ToProto(proto.Properties, value.Properties, context);
        }
    }
}
