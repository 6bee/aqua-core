// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Tests.TimeSpans;

public class When_converting_nanoseconds_to_a_time_span
{
    [Fact]
    public void Should_convert_to_ticks()
    {
        TimeSpanHelper.FromNanoseconds(1_500).ShouldBe(TimeSpan.FromTicks(15));
    }
}

public class When_checking_whether_ticks_fit_in_nanoseconds
{
    [Fact]
    public void Should_detect_values_inside_and_outside_the_supported_range()
    {
        TimeSpanHelper.FitsInNanoseconds(long.MaxValue / 100).ShouldBeTrue();
        TimeSpanHelper.FitsInNanoseconds(long.MinValue / 100).ShouldBeTrue();
        TimeSpanHelper.FitsInNanoseconds(long.MaxValue / 100 + 1).ShouldBeFalse();
        TimeSpanHelper.FitsInNanoseconds(long.MinValue / 100 - 1).ShouldBeFalse();
    }
}

public class When_converting_ticks_to_nanoseconds
{
    [Fact]
    public void Should_multiply_by_one_hundred()
    {
        TimeSpanHelper.TicksToNanoseconds(15).ShouldBe(1_500);
    }
}

public class When_converting_ticks_to_microseconds
{
    [Fact]
    public void Should_divide_by_ten()
    {
        TimeSpanHelper.TicksToMioseconds(15).ShouldBe(1);
    }
}

public class When_converting_microseconds_to_a_time_span
{
    [Fact]
    public void Should_convert_to_ticks()
    {
        TimeSpanHelper.FromMicroseconds(15).ShouldBe(TimeSpan.FromTicks(150));
    }
}
