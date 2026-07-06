// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf;

public sealed class ProtoContext
{
    public ProtoContext(ProtoOptions? options = null)
    {
        options ??= new();
        Tracker = options.Tracker;
        Resolver = options.Resolver;
    }

    public IReferenceTracker Tracker { get; }

    public IMapperResolver Resolver { get; }
}
