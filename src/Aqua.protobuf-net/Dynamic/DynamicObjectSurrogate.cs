// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf.Dynamic
{
    using Aqua.Dynamic;
    using Aqua.Extensions;
    using Aqua.TypeSystem;
    using global::ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    [ProtoContract(Name = nameof(DynamicObject))]
    public class DynamicObjectSurrogate : Value
    {
        public override Type ValueType => typeof(DynamicObject);

        public override object ObjectValue
        {
            get => Convert(this);
            set => throw new InvalidOperationException("Read-only proeprty may not be set.");
        }

        [ProtoMember(1)]
        public TypeInfo? Type { get; set; }

        [ProtoMember(2)]
        public Dictionary<string, Value?>? Properties { get; set; }

        [ProtoMember(3)]
        public Values? WrappedCollection { get; set; }

        [ProtoConverter]
        [return: NotNullIfNotNull("source")]
        public static DynamicObjectSurrogate? Convert(DynamicObject? source)
            => source is null
            ? null
            : source.IsSingleValueWrapper() && source.Values.Single().IsCollection(out var collection)
            ? new DynamicObjectSurrogate
                {
                    Type = source.Type,
                    WrappedCollection = Values.Wrap(collection, TypeHelper.GetElementType(source.Type?.Type) !),
                }
            : new DynamicObjectSurrogate
                {
                    Type = source.Type,
                    Properties = Map(source.Properties),
                };

        [ProtoConverter]
        [return: NotNullIfNotNull("surrogate")]
        public static DynamicObject? Convert(DynamicObjectSurrogate? surrogate)
            => surrogate is null
            ? null
            : surrogate.Properties.AsNullIfEmpty() is null
            ? new DynamicObject(surrogate.Type, Unwrap(surrogate.WrappedCollection))
            : new DynamicObject(surrogate.Type, Unwrap(surrogate.Properties!));

        [return: NotNullIfNotNull("wrappedCollection")]
        private static PropertySet? Unwrap(Values? wrappedCollection)
            => wrappedCollection is null
            ? null
            : new PropertySet(new[] { new Property(string.Empty, wrappedCollection.ObjectArray) });

        private static PropertySet Unwrap(Dictionary<string, Value?> properties)
            => new PropertySet(properties.Select(x => new Property(x.Key, x.Value?.ObjectValue)));

        private static object? UnwrapSingle(Value? element)
            => element?.ObjectValue;

        private static Dictionary<string, Value?> Map(PropertySet properties)
            => properties.ToDictionary(x => x.Name, x => Value.Wrap(x.Value));
    }
}
