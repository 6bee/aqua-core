// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Mappers;

using Aqua.TypeExtensions;
using Aqua.TypeSystem;
using Proto = Aqua.Protobuf.Schema;

public sealed class MemberInfoMapper : ProtoMapper<MemberInfo, Proto.MemberInfo>
{
    public static readonly MemberInfoMapper Instance = new();

    public override MemberInfo FromProto(Proto.MemberInfo proto, ProtoContext context)
    {
        if (proto is null)
        {
            return null!;
        }

        return proto.KindCase switch
        {
            Proto.MemberInfo.KindOneofCase.Property => PropertyInfoMapper.Instance.FromProto(proto.Property, context),
            Proto.MemberInfo.KindOneofCase.Field => FieldInfoMapper.Instance.FromProto(proto.Field, context),
            Proto.MemberInfo.KindOneofCase.Method => MethodInfoMapper.Instance.FromProto(proto.Method, context),
            Proto.MemberInfo.KindOneofCase.Constructor => ConstructorInfoMapper.Instance.FromProto(proto.Constructor, context),
            _ => throw new NotSupportedException($"MemberInfo kind {proto.KindCase} is not supported"),
        };
    }

    public override Proto.MemberInfo ToProto(MemberInfo value, ProtoContext context)
        => value switch
        {
            null => null!,
            PropertyInfo v => new() { Property = PropertyInfoMapper.Instance.ToProto(v, context) },
            FieldInfo v => new() { Field = FieldInfoMapper.Instance.ToProto(v, context) },
            MethodInfo v => new() { Method = MethodInfoMapper.Instance.ToProto(v, context) },
            ConstructorInfo v => new() { Constructor = ConstructorInfoMapper.Instance.ToProto(v, context) },
            _ => throw new NotSupportedException($"MemberInfo type {value?.GetType().GetFriendlyName()} is not supported"),
        };
}
