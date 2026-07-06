// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Mappers;

using Aqua.TypeSystem;
using Proto = Aqua.Protobuf.Schema;

public sealed class TypeInfoMapper : ProtoMapper<TypeInfo, Proto.TypeInfo>
{
    public static readonly TypeInfoMapper Instance = new();

    public override TypeInfo FromProto(Proto.TypeInfo proto, ProtoContext context)
        => proto is null ? null! : new()
        {
            Name = proto.Name.Length == 0 ? null : proto.Name,
            Namespace = proto.Namespace.Length == 0 ? null : proto.Namespace,
            DeclaringType = FromProto(proto.DeclaringType, context),
            IsAnonymousType = proto.IsAnonymousType,
            IsGenericType = proto.IsGenericType,
            GenericArguments = this.FromProto(proto.GenericArguments, context).ToListOrNull(),
            Properties = PropertyInfoMapper.Instance.FromProto(proto.Properties, context).ToListOrNull(),
        };

    public override Proto.TypeInfo ToProto(TypeInfo value, ProtoContext context)
    {
        if (value is null)
        {
            return null!;
        }

        var result = new Proto.TypeInfo
        {
            DeclaringType = ToProto(value.DeclaringType!, context),
            IsAnonymousType = value.IsAnonymousType,
            IsGenericType = value.IsGenericType,
        };
        if (value.Name?.Length > 0)
        {
            result.Name = value.Name;
        }

        if (value.Namespace?.Length > 0)
        {
            result.Namespace = value.Namespace;
        }

        this.ToProto(result.GenericArguments, value.GenericArguments, context);
        PropertyInfoMapper.Instance.ToProto(result.Properties, value.Properties, context);
        return result;
    }
}
