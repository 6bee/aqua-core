// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf;

public sealed class ProtoContext(ProtoOptions? options = null)
{
    private readonly ProtoOptions _options = options ?? new();
    private SerializationReferenceTracker? _serializationTracker;
    private DeserializationReferenceTracker? _deserializationTracker;

    public ProtoOptions Options => _options;

    public ISerializationReferenceTracker SerializationTracker => _serializationTracker ??= new(_options.ReferenceHandler);

    public IDeserializationReferenceTracker DeserializationTracker => _deserializationTracker ??= new();

    public IMapperResolver Resolver => _options.Resolver;

    public static implicit operator ProtoContext(ProtoOptions? options) => new(options);
}
