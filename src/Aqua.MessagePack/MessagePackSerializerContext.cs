// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack;

public class MessagePackSerializerContext(ReferenceHandler referenceHandler = ReferenceHandler.Unspecified)
{
    internal static MessagePackSerializerContext Default => new();

    private SerializationReferenceTracker? _serializationTracker;
    private DeserializationReferenceTracker? _deserializationTracker;

    public ReferenceHandler ReferenceHandler { get; } = referenceHandler;

    public ISerializationReferenceTracker SerializationTracker => _serializationTracker ??= new(ReferenceHandler);

    public IDeserializationReferenceTracker DeserializationTracker => _deserializationTracker ??= new();
}
