// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack;

/// <summary>
/// The <see cref="ReferenceHandler"/> to be used at run time.
/// </summary>
public enum ReferenceHandler
{
    /// <summary>
    /// Specifies that circular references should throw exceptions.
    /// </summary>
    Unspecified = 0,

    /// <summary>
    /// Specifies that the built-in <see cref="ReferenceHandler.Preserve"/> be used to handle references.
    /// </summary>
    Preserve = 1,

    /// <summary>
    /// Specifies that the built-in <see cref="ReferenceHandler.IgnoreCycles"/> be used to ignore cyclic references.
    /// </summary>
    IgnoreCycles = 2,
}
