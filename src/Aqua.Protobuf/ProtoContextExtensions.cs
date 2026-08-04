// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf;

using Aqua.Protobuf.Mappers;
using Aqua.Protobuf.Schema;
using Aqua.TypeExtensions;
using System.Buffers;
using System.ComponentModel;
using System.IO;
using NullValue = Google.Protobuf.WellKnownTypes.NullValue;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class ProtoContextExtensions
{
    extension(ProtoContext context)
    {
        /// <summary>
        /// Serializes <typeparamref name="T"/> to an <see cref="IMessage"/>.
        /// </summary>
        public IMessage ToProto<T>(T value)
        {
            var mapper = context.Resolver.GetMapperWithVerify<T>();
            return mapper.ToProto(value, context);
        }

        /// <summary>
        /// Serializes <typeparamref name="T"/> to an <see cref="IMessage"/> and cast the result to <typeparamref name="TProto"/>.
        /// </summary>
        public TProto ToProto<T, TProto>(T value)
            where TProto : IMessage
        {
            return (TProto)context.ToProto(value);
        }

        /// <summary>
        /// Deserializes <typeparamref name="T"/> from the given <paramref name="message"/>.
        /// </summary>
        public T FromProto<T>(IMessage message)
        {
            var mapper = context.Resolver.GetMapperWithVerify<T>();
            return mapper.FromProto(message, context);
        }

        /// <summary>
        /// Serializes <typeparamref name="T"/> to a byte array.
        /// </summary>
        public byte[] Serialize<T>(T value)
        {
            return context.ToProto(value).ToByteArray();
        }

        /// <summary>
        /// Serializes <typeparamref name="T"/> to the given <paramref name="span"/>.
        /// </summary>
        public void Serialize<T>(T value, Span<byte> span)
        {
            context.ToProto(value).WriteTo(span);
        }

        /// <summary>
        /// Serializes <typeparamref name="T"/> to the given <paramref name="writer"/>.
        /// </summary>
        public void Serialize<T>(T value, IBufferWriter<byte> writer)
        {
            context.ToProto(value).WriteTo(writer);
        }

        /// <summary>
        /// Serializes <typeparamref name="T"/> to the given <paramref name="stream"/>.
        /// </summary>
        public void Serialize<T>(T value, Stream stream)
        {
            context.ToProto(value).WriteTo(stream);
        }

        /// <summary>
        /// Deserializes <typeparamref name="T"/> from the given <paramref name="data"/>.
        /// </summary>
        public T Deserialize<T>(byte[] data)
        {
            data.AssertNotNull();

            var mapper = context.Resolver.GetMapperWithVerify<T>();
            var parser = mapper.GetMessageParser();
            var message = parser.ParseFrom(data);
            return mapper.FromProto(message, context);
        }

        /// <summary>
        /// Deserializes <typeparamref name="T"/> from the given <paramref name="data"/>.
        /// </summary>
        public T Deserialize<T>(ReadOnlySequence<byte> data)
        {
            var mapper = context.Resolver.GetMapperWithVerify<T>();
            var parser = mapper.GetMessageParser();
            var message = parser.ParseFrom(data);
            return mapper.FromProto(message, context);
        }

        /// <summary>
        /// Deserializes <typeparamref name="T"/> from the given <paramref name="data"/>.
        /// </summary>
        public T Deserialize<T>(ReadOnlySpan<byte> data)
        {
            var mapper = context.Resolver.GetMapperWithVerify<T>();
            var parser = mapper.GetMessageParser();
            var message = parser.ParseFrom(data);
            return mapper.FromProto(message, context);
        }

        /// <summary>
        /// Deserializes <typeparamref name="T"/> from the given <paramref name="stream"/>.
        /// </summary>
        public T Deserialize<T>(Stream stream)
        {
            stream.AssertNotNull();

            var mapper = context.Resolver.GetMapperWithVerify<T>();
            var parser = mapper.GetMessageParser();
            var message = parser.ParseFrom(stream);
            return mapper.FromProto(message, context);
        }

        /// <summary>
        /// Creates a value proto and a reference proto type for the specified <paramref name="value"/> or,
        /// if <see cref="ReferenceHandler.Preserve"/> and the value can be substituted, creates a reference proto with the reference proto set instead of the value.
        /// </summary>
        /// <typeparam name="TReferenceProto">The reference proto type to be returned.</typeparam>
        /// <typeparam name="TProto">The value proto type that carries to values payload.</typeparam>
        /// <typeparam name="T">The reference value type.</typeparam>
        /// <param name="value">The reference value to be written to proto data.</param>
        /// <param name="set">The callback for populating the value proto payload.</param>
        /// <returns>The reference proto.</returns>
        public TReferenceProto ToReferenceProto<TReferenceProto, TProto, T>(T value, Action<TProto, T, ProtoContext> set)
            where TReferenceProto : class, IReferenceProto<TProto>, new()
            where TProto : class, IHaveId, new()
            where T : class
        {
            if (value is null)
            {
                return new() { Null = NullValue.NullValue };
            }

            var tracker = context.SerializationTracker;
            using (tracker.Scope())
            {
                if (tracker.TryRegister(value, out var id))
                {
                    // write value
                    var proto = new TProto { Id = id };
                    set(proto, value, context);
                    return new() { Value = proto };
                }

                if (tracker.ReferenceHandler is ReferenceHandler.Preserve)
                {
                    // write reference
                    return new() { Ref = new() { Id = id } };
                }

                // ignore cycle
                return null!;
            }
        }

        /// <summary>
        /// Resolves a reference proto.
        /// </summary>
        /// <typeparam name="T">The reference type to be resolved.</typeparam>
        /// <param name="reference">The reference proto to be resolved.</param>
        /// <returns>The reference value that was resolved.</returns>
        public T Resolve<T>(Ref reference)
            where T : class
            => context.DeserializationTracker.Resolve<T>(reference.Id);

        /// <summary>
        /// Resolves a reference value from a value proto.
        /// </summary>
        /// <typeparam name="T">The reference type to be resolved.</typeparam>
        /// <typeparam name="TProto">The value proto tye.</typeparam>
        /// <param name="proto">The value proto to be read from</param>
        /// <param name="set">The callback action to populate the reference value payload from the value proto.</param>
        /// <returns>The resolved reference value.</returns>
        public T Resolve<T, TProto>(TProto proto, Action<T, TProto, ProtoContext> set)
            where T : class, new()
            where TProto : class, IHaveId
        {
            var value = new T();
            context.DeserializationTracker.Register(value, proto.Id);
            set(value, proto, context);
            return value;
        }
    }

    extension<T>(IProtoMapper<T> mapper)
    {
        private MessageParser GetMessageParser()
        {
            if (!mapper.GetType().Implements(typeof(IProtoMapper<,>), out var typeArgs))
            {
                throw null!; // unreachable
            }

            var protoType = typeArgs[1];
            var parserProperty = protoType.GetProperty("Parser")!;
            var parser = (MessageParser)parserProperty.GetValue(null)!;
            return parser;
        }
    }
}
