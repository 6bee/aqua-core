// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Mappers;

using Google.Protobuf.Collections;
using System.ComponentModel;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class ProtoMapperExtensions
{
    extension<T, TProto>(IProtoMapper<T, TProto> mapper)
        where TProto : IMessage
    {
        public T[] FromProto(RepeatedField<TProto> protos, ProtoContext context)
        {
            var result = new T[protos.Count];
            for (var i = 0; i < protos.Count; i++)
            {
                result[i] = mapper.FromProto(protos[i], context);
            }

            return result;
        }

        public void ToProto(RepeatedField<TProto> protos, IEnumerable<T>? items, ProtoContext context)
        {
            protos.AssertNotNull();

            if (items is null)
            {
                return;
            }

            foreach (var item in items)
            {
                protos.Add(mapper.ToProto(item, context));
            }
        }

        public void ToProto(RepeatedField<TProto> protos, System.Collections.IEnumerable? items, ProtoContext context)
        {
            protos.AssertNotNull();

            if (items is null)
            {
                return;
            }

            foreach (T item in items)
            {
                protos.Add(mapper.ToProto(item, context));
            }
        }
    }
}
