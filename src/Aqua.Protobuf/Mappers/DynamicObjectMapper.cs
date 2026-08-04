// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Mappers;

using Aqua.Dynamic;
using Google.Protobuf.Collections;
using static Aqua.Protobuf.Schema.DynamicObject;
using Proto = Aqua.Protobuf.Schema;

public sealed class DynamicObjectMapper : ProtoMapper<DynamicObject, Proto.DynamicObject>
{
    public static readonly DynamicObjectMapper Instance = new();

    public override DynamicObject FromProto(Proto.DynamicObject proto, ProtoContext context)
    {
        return proto?.NodeCase switch
        {
            null or
            NodeOneofCase.Null => null!,
            NodeOneofCase.Value => context.Resolve<DynamicObject, Proto.DynamicObjectValue>(proto.Value, FromProto),
            NodeOneofCase.Ref => context.Resolve<DynamicObject>(proto.Ref),
            _ => throw new NotSupportedException($"{proto.NodeCase} is not supported"),
        };

        static void FromProto(DynamicObject value, Proto.DynamicObjectValue proto, ProtoContext context)
        {
            value.Type = TypeInfoMapper.Instance.FromProto(proto.Type, context);
            value.Properties = proto.HasProperties ? FromPropertySet(proto.Properties, context) : null;

            static PropertySet FromPropertySet(MapField<string, Proto.Value> value, ProtoContext context)
            {
                var properties = new List<Property>(value.Count);
                foreach (var item in value)
                {
                    properties.Add(new(item.Key, ValueMapper.Instance.FromProto(item.Value, context)));
                }

                return new(properties);
            }
        }
    }

    public override Proto.DynamicObject ToProto(DynamicObject value, ProtoContext context)
    {
        return context.ToReferenceProto<Proto.DynamicObject,Proto.DynamicObjectValue,  DynamicObject>(value, ToProto);

        static void ToProto(Proto.DynamicObjectValue proto, DynamicObject value, ProtoContext context)
        {
            proto.Type = TypeInfoMapper.Instance.ToProto(value.Type!, context);

            proto.HasProperties = value.Properties is not null;

            if (value.Properties is { } properties)
            {
                foreach (var property in properties)
                {
                    proto.Properties.Add(property.Name, ValueMapper.Instance.ToProto(property.Value, context));
                }
            }
        }
    }
}
