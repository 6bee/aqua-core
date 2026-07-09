// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf;

public enum TimeSpanEncoding
{
    /// <summary>
    /// Serialize timespan as a signed int64 representing microseconds.
    /// </summary>
    Microseconds,

    /// <summary>
    /// Serialize timespan as a signed int64 representing Unix nanoseconds.
    /// </summary>
    Nanoseconds,

    /// <summary>
    /// Automatically selects the most suitable encoding.
    /// Serializes the timespan value as a signed int64 representing either
    /// microseconds or nanoseconds, preceded by a byte indicating
    /// the selected encoding:
    /// <list type="bullet">
    /// <item>1: Microseconds</item>
    /// <item>2: Nanoseconds</item>
    /// </list>
    /// Nanoseconds is used when the value requires nanosecond precision
    /// and fits within the nanoseconds range. Microseconds is used otherwise.
    /// </summary>
    Auto,
}
