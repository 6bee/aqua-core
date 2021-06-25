// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf.Dynamic
{
    using Aqua.Dynamic;
    using global::ProtoBuf;

    [ProtoContract(Name = nameof(Property))]
    public sealed class PropertySurrogate
    {
        [ProtoMember(1)]
        public string Name { get; set; } = null!;

        [ProtoMember(2)]
        public Value? Value { get; set; }

        [ProtoConverter]
        public static PropertySurrogate? Convert(Property? source)
            => source is null
            ? null
            : new PropertySurrogate
            {
                Name = source.Name,
                Value = Value.Wrap(source.Value),
            };

        [ProtoConverter]
        public static Property? Convert(PropertySurrogate? surrogate)
            => surrogate is null
            ? null
            : new Property(surrogate.Name, surrogate.Value?.ObjectValue);
    }
}
