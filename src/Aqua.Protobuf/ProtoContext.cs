// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf;

public sealed class ProtoContext
{
    private readonly ProtoOptions _options;
    private readonly SerializationReferenceTracker? _serializationTracker;
    private readonly DeserializationReferenceTracker? _deserializationTracker;

    private ProtoContext(bool isWrite, ProtoOptions? options = null)
    {
        _options = options ?? new();
        if (isWrite)
        {
            _serializationTracker = new SerializationReferenceTracker(_options.ReferenceHandler);
        }
        else
        {
            _deserializationTracker = new DeserializationReferenceTracker();
        }
    }

    public static ProtoContext ForRead(ProtoOptions? options = null) => new(false, options);

    public static ProtoContext ForWrite(ProtoOptions? options = null) => new(true, options);

    public ProtoOptions Options => _options;

    public ISerializationReferenceTracker SerializationTracker
        => _serializationTracker ?? throw new InvalidOperationException("Context is configured for read.");

    public IDeserializationReferenceTracker DeserializationTracker
        => _deserializationTracker ?? throw new InvalidOperationException("Context is configured for write.");

    public IMapperResolver Resolver => _options.Resolver;
}
