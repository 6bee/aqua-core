// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization
{
    using Aqua.EnumerableExtensions;
    using Aqua.ProtoBuf;
    using Aqua.TypeExtensions;
    using System;
    using System.Linq;
    using System.Numerics;
    using Xunit;

    public static class ProtobufNetSerializationHelper
    {
        private static readonly global::ProtoBuf.Meta.TypeModel _configuration = CreateTypeModel();

        private static global::ProtoBuf.Meta.TypeModel CreateTypeModel()
        {
            var configuration = ProtoBufTypeModel.ConfigureAquaTypes();

            var testdatatypes = TestData.TestTypes
                .Select(x => (Type)x[0])
                .Select(x => x.AsNonNullableType())
                .Distinct()
                .ToArray();
            testdatatypes.ForEach(x => configuration.AddDynamicPropertyType(x));

            return configuration;
        }

        public static T Clone<T>(this T graph) => Clone(graph, null);

        public static T Clone<T>(this T graph, global::ProtoBuf.Meta.TypeModel model)
            => (T)(model ?? _configuration).DeepClone(graph);

        public static void SkipUnsupportedDataType(Type type, object value)
        {
            Skip.If(type.Is<DateTimeOffset>(), $"{type} not supported by out-of-the-box protobuf-net");
            Skip.If(type.Is<BigInteger>(), $"{type} not supported by out-of-the-box protobuf-net");
            Skip.If(type.Is<Complex>(), $"{type} not supported by out-of-the-box protobuf-net");
            Skip.If(type.IsNotPublic(), $"Not-public {type} not supported protobuf-net");
#if NET5_0_OR_GREATER
            Skip.If(type.Is<Half>(), $"{type} serialization is not supported.");
#endif // NET5_0_OR_GREATER
        }
    }
}