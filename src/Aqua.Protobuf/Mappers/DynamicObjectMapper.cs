// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Mappers;

using Aqua.Dynamic;
using Google.Protobuf.Collections;
using Proto = Aqua.Protobuf.Schema;

public sealed class DynamicObjectMapper : ProtoMapper<DynamicObject, Proto.DynamicObject>
{
    public static readonly DynamicObjectMapper Instance = new();

    public override DynamicObject FromProto(Proto.DynamicObject proto, ProtoContext context)
    {
        if (proto is null)
        {
            return null!;
        }

        var type = TypeInfoMapper.Instance.FromProto(proto.Type, context);
        var properties = proto.HasProperties ? FromPropertySet(proto.Properties, context) : null;
        return new DynamicObject(type, properties);
    }

    public override Proto.DynamicObject ToProto(DynamicObject value, ProtoContext context)
    {
        if (value is null)
        {
            return null!;
        }

        var result = new Proto.DynamicObject
        {
            Type = TypeInfoMapper.Instance.ToProto(value.Type!, context),
            HasProperties = value.Properties is not null,
        };

        if (value.Properties is { } properties)
        {
            foreach (var property in properties)
            {
                result.Properties.Add(property.Name, ValueMapper.Instance.ToProto(property.Value, context));
            }
        }

        return result;
    }

    private static PropertySet FromPropertySet(MapField<string, Proto.Value> value, ProtoContext context)
    {
        var properties = new List<Property>(value.Count);
        foreach (var item in value)
        {
            properties.Add(new(item.Key, ValueMapper.Instance.FromProto(item.Value, context)));
        }

        return new(properties);
    }
}
