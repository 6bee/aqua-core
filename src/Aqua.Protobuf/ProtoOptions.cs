// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf;

public sealed record class ProtoOptions
{
    public DateTimeEncoding DateTimeEncoding { get; init; }

    public TimeSpanEncoding TimeSpanEncoding { get; init; }

    /// <summary>
    /// Gets the <see cref="ReferenceHandler"/> to use for serialization.
    /// </summary>
    /// <remarks>
    /// This setting is applied only during serialization and is ignored during deserialization.
    /// </remarks>
    public ReferenceHandler ReferenceHandler { get; init; }

    public IMapperResolver Resolver
    {
        get => field ??= MapperResolver.Empty.AddAquaTypes().Optimized();
        init => field = value.CheckNotNull();
    }
}
