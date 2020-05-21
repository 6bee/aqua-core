// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if COREFX

namespace Aqua.Tests.Serialization
{
    using global::ProtoBuf.Meta;
    using System;
    using System.Collections;
    using System.Linq;
    using System.Numerics;
    using Xunit;

    public static class ProtobufNetSerializationHelper
    {
        private static readonly RuntimeTypeModel _configuration = RuntimeTypeModel.Create().ConfigureAquaTypes();

        public static T Serialize<T>(this T graph) => (T)_configuration.DeepClone(graph);

        public static void SkipUnsupportedDataType(Type type, object value)
        {
            Skip.If(type.Is<DateTimeOffset>(), "Data type not supported by out-of-the-box protobuf-net");
            Skip.If(type.Is<BigInteger>(), "Data type not supported by out-of-the-box protobuf-net");
            Skip.If(type.Is<Complex>(), "Data type not supported by out-of-the-box protobuf-net");

            // TODO: check protobuf-net doc
            Skip.If(type.IsCollection() && ((IEnumerable)value).Cast<object>().All(x => x is null), "Collections with only null elements are set null by out-of-the-box protobuf-net");
        }
    }
}

#endif // COREFX
