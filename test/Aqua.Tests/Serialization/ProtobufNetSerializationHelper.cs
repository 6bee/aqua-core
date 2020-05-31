// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization
{
    using System;
    using System.Numerics;
    using Xunit;

    public static class ProtobufNetSerializationHelper
    {
#if COREFX
        private static readonly global::ProtoBuf.Meta.TypeModel _configuration = global::ProtoBuf.Meta.RuntimeTypeModel.Create().ConfigureAquaTypes();

        public static T Serialize<T>(this T graph) => (T)Serialize(graph, null);

        public static T Serialize<T>(this T graph, global::ProtoBuf.Meta.TypeModel model) => (T)(model ?? _configuration).DeepClone(graph);
#endif // COREFX

        public static void SkipUnsupportedDataType(Type type)
        {
            Skip.If(type.Is<DateTimeOffset>(), "Data type not supported by out-of-the-box protobuf-net");
            Skip.If(type.Is<BigInteger>(), "Data type not supported by out-of-the-box protobuf-net");
            Skip.If(type.Is<Complex>(), "Data type not supported by out-of-the-box protobuf-net");
            Skip.If(type.IsEnum(), "Data type not supported by out-of-the-box protobuf-net");
        }
    }
}
