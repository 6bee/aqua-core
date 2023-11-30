// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Dynamic;

using System;

public interface IIsKnownTypeProvider
{
    /// <summary>
    /// Returns a boolean value indicating whether the type specified is known.
    /// </summary>
    /// <param name="type">The type to be examined.</param>
    bool IsKnownType(Type type);
}