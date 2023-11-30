// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization;

using Shouldly;
using System;
using Xunit;

public abstract class When_serializing
{
    public class With_system_text_json_serializer : When_serializing
    {
        protected override T Serialize<T>(T value)
            => SystemTextJsonSerializationHelper.Clone(value);
    }

    public class With_newtonsoft_json_serializer : When_serializing
    {
        protected override T Serialize<T>(T value)
            => NewtonsoftJsonSerializationHelper.Clone(value);
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

    private static TimeSpan CreateTimeSpan()
        => new DateTime(DateTime.Now.Year, 1, 1) - DateTime.Now;

    protected abstract T Serialize<T>(T value);
}