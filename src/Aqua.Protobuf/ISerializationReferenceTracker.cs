// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf;

public interface ISerializationReferenceTracker
{
    ReferenceHandler ReferenceHandler { get; }

    /// <summary>
    /// Register a reference value before writing serialization data, taking <see cref="ReferenceHandler"/> setting into account.
    /// </summary>
    /// <typeparam name="T">The reference type of the value to be registered.</typeparam>
    /// <param name="value">The value to be written.</param>
    /// <param name="id">The ID of the reference value for <see cref="ReferenceHandler.Preserve"/>, <c>0</c> otherwise.</param>
    /// <returns>Returns <see langword="true"/> if the value shall be written, <see langword="false"/> otherwise.
    /// When return value is <see langword="false"/> and <see cref="ReferenceHandler.Preserve"/>, the value reference (substitution)
    /// shall be written using <paramref name="id"/> instead of the value.</returns>
    bool TryRegister<T>(T value, out uint id) where T : class;

    /// <summary>
    /// Creates a serialization scope for path-based (ancestor-chain) cycle tracking.
    /// Must be called <i>before</i> <see cref="TryRegister{T}"/> so that the snapshot
    /// of the current ancestor chain is taken on a clean parent; <see cref="IDisposable.Dispose"/>
    /// restores the parent state so siblings are unaffected.
    /// Required for <see cref="ReferenceHandler.Unspecified"/> and <see cref="ReferenceHandler.IgnoreCycles"/>;
    /// returns a no-op disposable for <see cref="ReferenceHandler.Preserve"/>.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> that must be disposed at the end of the scope.</returns>
    IDisposable Scope();
}
