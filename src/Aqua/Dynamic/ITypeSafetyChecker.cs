// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Dynamic;

/// <summary>
/// Denotes a type that allows to assert type safety for instace creation on mapping from <see cref="DynamicObject"/> in reference to
/// <see href="https://owasp.org/www-project-top-ten/2017/A8_2017-Insecure_Deserialization">OWASP A8:2017-Insecure Deserialization</see>.
/// </summary>
public interface ITypeSafetyChecker
{
    /// <summary>
    /// Asserts the <see cref="Type"/> specified is safe for instanciation.
    /// </summary>
    /// <param name="type">The type to be deserialized.</param>
    void AssertTypeSafety(Type type);
}
