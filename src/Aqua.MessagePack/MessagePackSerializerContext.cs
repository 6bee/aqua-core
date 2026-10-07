// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack;

/// <summary>
/// Holds state used to serialize and deserialize a logical MessagePack operation unit.
/// </summary>
/// <param name="referenceHandler">
/// The <see cref="ReferenceHandler"/> to use for serialization. This value is ignored during deserialization.
/// </param>
/// <remarks>
/// Instances are not thread-safe. Create a new instance per logical operation unit and do not share across threads.
/// </remarks>
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
