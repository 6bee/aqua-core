// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization;

using System.Numerics;

public abstract class When_serializing
{
    public class With_system_text_json_serializer : When_serializing
    {
        protected override T Serialize<T>(T value)
        {
            if (value is not null)
            {
                SystemTextJsonSerializationHelper.SkipUnsupportedDataType(value.GetType(), value);
            }

            return SystemTextJsonSerializationHelper.Clone(value);
        }
    }

    public class With_newtonsoft_json_serializer : When_serializing
    {
        protected override T Serialize<T>(T value) => NewtonsoftJsonSerializationHelper.Clone(value);
    }

    public class With_messagepack_serializer : When_serializing
    {
        protected override T Serialize<T>(T value) => MessagePackSerializationHelper.Clone(value);
    }

    public class With_protobuf_serializer : When_serializing
    {
        protected override T Serialize<T>(T value) => ProtobufSerializationHelper.Clone(value);
    }

    [Fact]
    public void Should_rountrip_string()
    {
        var value = "test string";

        var copy = Serialize(value);

        copy.ShouldBe(value);
    }

    [Fact]
    public void Should_rountrip_decimal()
    {
        var value = 0.000000003333308m;

        var copy = Serialize(value);

        copy.ShouldBe(value);
    }

    [Fact]
    public void Should_rountrip_int()
    {
        var value = 1;

        var copy = Serialize(value);

        copy.ShouldBe(value);
    }

    [Fact]
    public void Should_rountrip_bigint()
    {
        var value = new BigInteger(123008L);

        var copy = Serialize(value);

        copy.ShouldBe(value);
    }

    [Fact]
    public void Should_rountrip_guid()
    {
        var guid = Guid.NewGuid();

        var copy = Serialize(guid);

        copy.ShouldBe(guid);
    }

    [Fact]
    public void Should_rountrip_timespan()
    {
        var timespan = CreateTimeSpan();

        var copy = Serialize(timespan);

        copy.ShouldBe(timespan);
    }

    [Fact]
    public void Should_rountrip_nullable_timespan_with_null()
    {
        TimeSpan? timespan = null;

        var copy = Serialize(timespan);

        copy.ShouldBeNull();
    }

    [Fact]
    public void Should_rountrip_nullable_timespan()
    {
        TimeSpan? timespan = CreateTimeSpan();

        var copy = Serialize(timespan);

        copy.ShouldBe(timespan.Value);
    }

    private static TimeSpan CreateTimeSpan() => (new DateTime(DateTime.Now.Year, 1, 1) - DateTime.Now).ToMicrosecondPrecision();

    protected abstract T Serialize<T>(T value);
}
