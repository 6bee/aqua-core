// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Dynamic;

/// <summary>
/// Denotes a type that allows to assert type safety for instace creation on mapping from <see cref="DynamicObject"/> in reference to OWASP A8:2017-Insecure Deserialization.
/// </summary>
public interface ITypeSafetyChecker
{
    /// <summary>
    /// Asserts the <see cref="Type"/> specified is safe for instanciation.
    /// </summary>
    /// <param name="type">The type to be deserialized.</param>
    void AssertTypeSafety(Type type);
}