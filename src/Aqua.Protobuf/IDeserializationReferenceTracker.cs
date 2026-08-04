// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf;

public interface IDeserializationReferenceTracker
{
    /// <summary>
    /// Register a reference value created when reading from serialized data.
    /// </summary>
    /// <typeparam name="T">The reference type of the value to be registered.</typeparam>
    /// <param name="value">The value created from serialized data.</param>
    /// <param name="id">The ID representing the reference value.</param>
    void Register<T>(T value, uint id) where T : class;

    /// <summary>
    /// Resolves the reference value for the given ID when reading a value reference from serialized data.
    /// </summary>
    /// <typeparam name="T">The reference type of the value to be resolved.</typeparam>
    /// <param name="id">The ID representing the reference value.</param>
    /// <returns>The reference value resolved for the given ID.</returns>
    T Resolve<T>(uint id) where T : class;
}
