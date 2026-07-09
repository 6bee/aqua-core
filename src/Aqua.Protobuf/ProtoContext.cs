// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf;

public sealed class ProtoContext(ProtoOptions? options = null)
{
    private readonly ProtoOptions _options = options ?? new();

    public ProtoOptions Options => _options;

    public IReferenceTracker Tracker => _options.Tracker;

    public IMapperResolver Resolver => _options.Resolver;
}
