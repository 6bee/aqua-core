// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf.Tests;

public static class AquaProtobufExtensions
{
    extension(DateTime value)
    {
        public DateTime ToMicrosecondPrecision() => new(value.Ticks / 10 * 10, value.Kind);

#if NETFRAMEWORK
        public static DateTime UnixEpoch => new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
#endif // NETFRAMEWORK
    }

    extension(DateTimeOffset value)
    {
        public DateTimeOffset ToMicrosecondPrecision() => new(value.Ticks / 10 * 10, TimeSpan.Zero);
    }

    extension(TimeSpan value)
    {
        public TimeSpan ToMicrosecondPrecision() => TimeSpan.FromTicks(value.Ticks / 10 * 10);
    }
}
