// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization;

internal static class Extensions
{
    extension(DateTime value)
    {
        public DateTime ToMicrosecondPrecision() => new(value.Ticks / 10 * 10, value.Kind);
    }

    extension(DateTimeOffset value)
    {
        public DateTimeOffset ToMicrosecondPrecision() => new(value.Ticks / 10 * 10, TimeSpan.Zero);
    }

    extension(TimeSpan value)
    {
        public TimeSpan ToMicrosecondPrecision() => TimeSpan.FromTicks(value.Ticks / 10 * 10);
    }

#if NET6_0_OR_GREATER
    extension(TimeOnly value)
    {
        public TimeOnly ToMicrosecondPrecision() => new(value.Ticks / 10 * 10);
    }
#endif // NET6_0_OR_GREATER
}
