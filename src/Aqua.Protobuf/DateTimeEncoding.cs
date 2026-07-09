// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf;

public enum DateTimeEncoding
{
    /// <summary>
    /// Serialize datetime as a signed int64 representing Unix microseconds
    /// elapsed since 1970-01-01T00:00:00Z.
    /// </summary>
    /// <remarks>
    /// Range: Full range of <see cref="DateTime"/>.
    /// <br/>
    /// Theorectical range: approximately 290,301 BCE to 294,241 CE.
    /// </remarks>
    UnixMicroseconds,

    /// <summary>
    /// Serialize datetime as a signed int64 representing Unix nanoseconds
    /// elapsed since 1970-01-01T00:00:00Z.
    /// </summary>
    /// <remarks>
    /// Range: 1677-09-21T00:12:43.145224192Z to 2262-04-11T23:47:16.854775807Z.
    /// </remarks>
    UnixNanoseconds,

    /// <summary>
    /// Automatically selects the most suitable encoding.
    /// Serializes the datetime value as a signed int64 representing either
    /// Unix microseconds or Unix nanoseconds, preceded by a byte indicating
    /// the selected encoding:
    /// <list type="bullet">
    /// <item>1: Unix microseconds</item>
    /// <item>2: Unix nanoseconds</item>
    /// </list>
    /// Unix nanoseconds is used when the value requires nanosecond precision
    /// and fits within the Unix nanoseconds range. Unix microseconds is used
    /// otherwise.
    /// </summary>
    Auto,

    // /// <summary>
    // /// Uses an adaptive encoding for datetime values, writing a leading byte with the size,
    // /// followed by milliseconds (4 bytes), nanoseconds (8 bytes), or seconds + nanoseconds (8 + 4 bytes).
    // /// </summary>
    // /// <remarks>
    // /// Range:
    // /// <list type="bullet">
    // ///   <item>milliseconds (signed int32): approximately 1938-12-15 to 2001-01-19</item>
    // ///   <item>nanoseconds (signed int64): 1677-09-21T00:12:43.145224192Z to 2262-04-11T23:47:16.854775807Z</item>
    // ///   <item>seconds + nanos (signed int64 + unsigned int32): approximately 292,277,026,596 BCE to 292,277,026,596 CE</item>
    // /// </list>
    // /// </remarks>
    // Adaptive,
}
