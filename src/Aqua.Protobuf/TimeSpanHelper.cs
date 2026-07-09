// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf;

using System.Runtime.CompilerServices;

internal static class TimeSpanHelper
{
    private const long TicksPerMicrosecond = 10L;
    private const long NanosecondsPerTick = 100L;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeSpan FromNanoseconds(long nanoseconds) => TimeSpan.FromTicks(nanoseconds / NanosecondsPerTick);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool FitsInNanoseconds(long ticks) =>
        ticks <= long.MaxValue / NanosecondsPerTick &&
        ticks >= long.MinValue / NanosecondsPerTick;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long TicksToNanoseconds(long ticks) => ticks * NanosecondsPerTick;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long TicksToMioseconds(long ticks) => ticks / TicksPerMicrosecond;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeSpan FromMicroseconds(long microseconds) => TimeSpan.FromTicks(microseconds * TicksPerMicrosecond);
}
