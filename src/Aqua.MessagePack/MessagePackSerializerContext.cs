// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack;

/// <summary>
/// Provides serialization and deserialization reference trackers for MessagePack processing.
/// </summary>
/// <param name="referenceHandler">
/// The <see cref="ReferenceHandler"/> to use for serialization. This value is ignored during deserialization.
/// </param>
public class MessagePackSerializerContext(ReferenceHandler referenceHandler = ReferenceHandler.Unspecified)
{
    private SerializationReferenceTracker? _serializationTracker;
    private DeserializationReferenceTracker? _deserializationTracker;

    /// <summary>
    /// Gets the <see cref="ReferenceHandler"/> used for serialization.
    /// </summary>
    /// <remarks>
    /// This setting is applied only during serialization and is ignored by deserialization.
    /// </remarks>
    public ReferenceHandler ReferenceHandler { get; } = referenceHandler;

    public ISerializationReferenceTracker SerializationTracker => _serializationTracker ??= new(ReferenceHandler);

    public IDeserializationReferenceTracker DeserializationTracker => _deserializationTracker ??= new();
}
