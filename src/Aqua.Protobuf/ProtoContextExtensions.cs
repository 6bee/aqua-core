// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf;

using Aqua.Protobuf.Mappers;
using Aqua.TypeExtensions;
using System.Buffers;
using System.ComponentModel;
using System.IO;

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
