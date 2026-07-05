// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf;

internal static class DateTimeExtensions
{
    private const long UnixEpochTicks = 621355968000000000L; // DateTime.UnixEpoch.Ticks

    extension(DateTime utc)
    {
        public long ToUnixMicroseconds() => (utc.Ticks - UnixEpochTicks) / 10L;

        public static long TicksFromUnixMicroseconds(long unixMicroseconds) => (unixMicroseconds * 10L) + UnixEpochTicks;
    }
}
