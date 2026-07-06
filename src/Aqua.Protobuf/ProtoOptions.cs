// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf;

public sealed class ProtoOptions
{
    public IReferenceTracker Tracker
    {
        get => field ??= new ReferenceTracker();
        init => field = value.CheckNotNull();
    }

    public IMapperResolver Resolver
    {
        get => field ??= MapperResolver.Empty.AddAquaTypes().Optimized();
        init => field = value.CheckNotNull();
    }
}
