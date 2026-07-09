// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf;

using System.Runtime.CompilerServices;

internal static class DateTimeExtensions
{
    private const long UnixEpochTicks = 621355968000000000L; // DateTime.UnixEpoch.Ticks

    private const long UnixNanosecondsMinTicks = UnixEpochTicks + (long.MinValue / 100L);

    private const long UnixNanosecondsMaxTicks = UnixEpochTicks + (long.MaxValue / 100L);

    extension(DateTime utc)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ToUnixMicroseconds() => (utc.Ticks - UnixEpochTicks) / 10L;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ToUnixNanoseconds() => (utc.Ticks - UnixEpochTicks) * 100L;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long TicksFromUnixMicroseconds(long unixMicroseconds) => (unixMicroseconds * 10L) + UnixEpochTicks;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long TicksFromUnixNanoseconds(long unixNanoseconds) => (unixNanoseconds / 100L) + UnixEpochTicks;

        internal static long UnixNanosecondsMinTicks => UnixNanosecondsMinTicks;

        internal static long UnixNanosecondsMaxTicks => UnixNanosecondsMaxTicks;
    }
}
