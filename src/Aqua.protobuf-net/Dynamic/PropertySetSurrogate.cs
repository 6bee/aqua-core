// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.ProtoBuf.Dynamic
{
    using Aqua.Dynamic;
    using global::ProtoBuf;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    [ProtoContract]
    public sealed class PropertySetSurrogate
    {
        [ProtoMember(1, IsRequired = true)]
        public Property[] Properties { get; set; } = null!;

        [ProtoConverter]
        [return: NotNullIfNotNull("source")]
        public static PropertySetSurrogate? Convert(PropertySet? source)
            => source is null
            ? null
            : new PropertySetSurrogate { Properties = source.ToArray() };

        [ProtoConverter]
        [return: NotNullIfNotNull("surrogate")]
        public static PropertySet? Convert(PropertySetSurrogate? surrogate)
            => surrogate is null
            ? null
            : new PropertySet(surrogate.Properties);
    }
}