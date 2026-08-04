// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack;

using Aqua.Utils;

internal sealed class SerializationReferenceTracker(ReferenceHandler mode) : ISerializationReferenceTracker
{
    private readonly ReferenceHandler _mode = mode switch
    {
        ReferenceHandler.Unspecified or
        ReferenceHandler.Preserve or
        ReferenceHandler.IgnoreCycles => mode,
        _ => throw new InvalidOperationException($"Model {mode} is not supported"),
    };
    private Dictionary<object, int> _lookup = new(ReferenceEqualityComparer<object>.Default);
    private int _count;

    public ReferenceHandler ReferenceHandler => _mode;

    public bool TryRegister<T>(T value, out int id) where T : class
    {
        if (_mode is ReferenceHandler.Preserve)
        {
            if (_lookup.TryGetValue(value, out id))
            {
                return false; // don't write value
            }

            id = ++_count;
            _lookup[value] = id;
            return true; // write value
        }

        // Unspecified and IgnoreCycles: path-based (ancestor-chain) tracking.
        // Scope() snapshots and restores _lookup around each value so siblings
        // never observe each other — only true back-edges to an ancestor trigger detection.
        id = default;
        if (_lookup.ContainsKey(value))
        {
            if (_mode is ReferenceHandler.Unspecified)
            {
                throw new InvalidOperationException("A possible object cycle was detected.");
            }

            return false; // IgnoreCycles: suppress back-edge
        }

        _lookup[value] = id;
        return true; // write value
    }

    public IDisposable Scope()
    {
        // Preserve uses global flat tracking — no scope needed.
        // Unspecified and IgnoreCycles use path-based tracking: each scope
        // snapshots _lookup before entering a value and restores it on exit,
        // so only the current ancestor chain is visible to TryRegister.
        if (_mode is ReferenceHandler.Preserve)
        {
            return default!;
        }

        var lookup = _lookup;
        _lookup = new(lookup, ReferenceEqualityComparer<object>.Default);
        return new LookupScope(this, lookup);
    }

    private struct LookupScope(SerializationReferenceTracker tracker, Dictionary<object, int> lookup) : IDisposable
    {
        public void Dispose()
        {
            tracker?._lookup = lookup;
            tracker = null!;
        }
    }
}
