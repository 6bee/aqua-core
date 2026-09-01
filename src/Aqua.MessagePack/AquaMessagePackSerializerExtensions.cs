// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace MessagePack;
#pragma warning restore IDE0130 // Namespace does not match folder structure

using Aqua.MessagePack;
using System.ComponentModel;

/// <summary>
/// Extension methods to configure <see cref="MessagePackSerializerOptions"/> for serializing
/// <i>Aqua</i> types with type-safe formatters.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class AquaMessagePackSerializerExtensions
{
    public delegate void SerializeMembers<in T>(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options);

    public delegate void DeserializeMembers<in T>(ref MessagePackReader reader, uint fieldCount, T value, MessagePackSerializerOptions options);

    extension(MessagePackSerializerOptions options)
    {
        /// <summary>
        /// Returns a copy of the <see cref="MessagePackSerializerOptions"/> configured to serialize
        /// <i>Aqua</i> types using type-safe formatters, with <see cref="MessagePackSecurity.UntrustedData"/> applied.
        /// </summary>
        /// <returns>A configured <see cref="AquaMessagePackSerializerOptions"/> instance.</returns>
        public AquaMessagePackSerializerOptions ConfigureAqua()
            => options
            .CheckNotNull()
            .WithResolver(new AquaFormatterResolver(options.Resolver))
            .AsAquaMessagePackSerializerOptions();

        private AquaMessagePackSerializerOptions AsAquaMessagePackSerializerOptions()
            => options as AquaMessagePackSerializerOptions ?? new(options);

        /// <summary>
        /// Gets the msg pack serialization context. This requires <see cref="AquaMessagePackSerializerOptions"/> or a derived type.
        /// </summary>
        public MessagePackSerializerContext Context
            => options is AquaMessagePackSerializerOptions opts
            ? opts.Context
            : throw new InvalidOperationException($"Aqua type serialization requires {nameof(AquaMessagePackSerializerOptions)}");
    }

    extension(ref MessagePackWriter writer)
    {
        /// <summary>
        /// Serializes given reference <paramref name="value"/> or,
        /// if <see cref="ReferenceHandler.Preserve"/> and the value can be substituted, serialises a value reference instead of the value.
        /// </summary>
        /// <typeparam name="T">The reference value type.</typeparam>
        /// <param name="fieldCount">The number of fields to be serialized for the value.</param>
        /// <param name="value">The reference value to be serialized.</param>
        /// <param name="options">Serialization options.</param>
        /// <param name="serializeMembers">The callback for serializing the value members.</param>
        public void SerializeReferenceValue<T>(uint fieldCount, T? value, MessagePackSerializerOptions options, SerializeMembers<T> serializeMembers)
            where T : class
        {
            if (value is null)
            {
                writer.WriteNil();
                return;
            }


            var tracker = options.Context.SerializationTracker;
            using (tracker.Scope())
            {
                if (tracker.TryRegister(value, out var id))
                {
                    // write value
                    writer.WriteArrayHeader(fieldCount + 1);
                    writer.Write(id);

                    serializeMembers(ref writer, value, options);
                }
                else if (tracker.ReferenceHandler is ReferenceHandler.Preserve)
                {
                    // write reference
                    writer.WriteArrayHeader(1);
                    writer.Write(-id);
                }
                else
                {
                    // ignore cycle
                    writer.WriteNil();
                }
            }
        }
    }

    extension(ref MessagePackReader reader)
    {
        /// <summary>
        /// Deserializes the specified reference value of type <typeparamref name="T"/> from serialized data.
        /// If the value was serialized as value reference, the value is resolved from serialization context.
        /// </summary>
        /// <typeparam name="T">The reference value type.</typeparam>
        /// <param name="options">Serialization options.</param>
        /// <param name="deserializeMembers">The callback for deserializing the value members.</param>
        /// <returns>The deserialized or resolved reference value.</returns>
        public T? DeserializeReferenceValue<T>(MessagePackSerializerOptions options, DeserializeMembers<T> deserializeMembers)
            where T : class, new()
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            options.Security.DepthStep(ref reader);
            try
            {
                var fieldCount = reader.ReadArrayHeader() - 1;

                var tracker = options.Context.DeserializationTracker;
                var id = reader.ReadInt32();
                if (id < 0)
                {
                    if (fieldCount is not 0)
                    {
                        throw new MessagePackSerializationException("Field count must be zero for value reference");
                    }

                    return tracker.Resolve<T>(-id);
                }

                if (fieldCount < 0)
                {
                    throw new MessagePackSerializationException("Field count must not be negative");
                }

                var value = new T();
                tracker.Register(value, id);
                deserializeMembers(ref reader, (uint)fieldCount, value, options);
                return value;
            }
            finally
            {
                reader.Depth--;
            }
        }
    }
}
