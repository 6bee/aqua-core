// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Tests.AquaScalarCodec;

using Aqua.Protobuf;
using System.Buffers;

public abstract class When_encoding_datetime(DateTimeEncoding dateTimeEncoding)
{
    protected readonly ProtoOptions Options = new() { DateTimeEncoding = dateTimeEncoding };

    public class As_unix_microseconds() : When_encoding_datetime(DateTimeEncoding.UnixMicroseconds)
    {
        [Theory]
        [MemberData(nameof(TestValuesForMicrosecondsEncoding))]
        public void Should_rountrip(DateTime value)
        {
            var copy = RoundTrip(value);
            copy.ShouldBe(value);
        }
    }

    public class As_unix_nanoseconds() : When_encoding_datetime(DateTimeEncoding.UnixNanoseconds)
    {
        [Theory]
        [MemberData(nameof(TestValuesForNanosecondsEncoding))]
        public void Should_rountrip(DateTime value)
        {
            var copy = RoundTrip(value);
            copy.ShouldBe(value);
        }
    }

    public class With_auto_encoding() : When_encoding_datetime(DateTimeEncoding.Auto)
    {
        [Theory]
        [MemberData(nameof(TestValuesForAutoEncoding))]
        public void Should_rountrip(DateTime value)
        {
            var copy = RoundTrip(value);
            copy.ShouldBe(value);
        }
    }

    [Theory]
    [MemberData(nameof(TestValuesForAllEncodings))]
    public void Should_rountrip_any_encoding(DateTime value)
    {
        var copy = RoundTrip(value);
        copy.ShouldBe(value);
    }

    private DateTime RoundTrip(DateTime value)
    {
        var writer = new ArrayBufferWriter<byte>();

        // encode
        AquaScalarCodec.Encode(writer, value, Options);

        // decode
        var decoded = AquaScalarCodec.Decode(writer.WrittenSpan, typeof(DateTime), Options);

        return (DateTime)decoded;
    }

    public static IEnumerable<object[]> TestValuesForAllEncodings
    {
        get
        {
            // min and max value at max precision that sustain both encodings as millies as well as nanos
            yield return [DateTime.Parse("1677-09-21T00:12:43.145225Z").ToUniversalTime()];
            yield return [DateTime.Parse("2262-04-11T23:47:16.854775Z").ToUniversalTime()];
        }
    }

    public static IEnumerable<object[]> TestValuesForMicrosecondsEncoding
    {
        get
        {
            // min and max value at max precision that sustain microseconds encodings
            yield return [new DateTime(0, DateTimeKind.Utc)];
            yield return [new DateTime(DateTime.MaxValue.Ticks / 10 * 10, DateTimeKind.Utc)];

            // smalles datetime different from zero
            yield return [new DateTime(10, DateTimeKind.Utc)];
        }
    }

    public static IEnumerable<object[]> TestValuesForNanosecondsEncoding
    {
        get
        {
            // min and max value at max precision that sustain nanoseconds encodings
            yield return [DateTime.Parse("1677-09-21T00:12:43.145224192Z").ToUniversalTime()];
            yield return [DateTime.Parse("2262-04-11T23:47:16.854775807Z").ToUniversalTime()];
        }
    }

    public static IEnumerable<object[]> TestValuesForAutoEncoding
        => TestValuesForMicrosecondsEncoding.Concat(TestValuesForNanosecondsEncoding);
}
