// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Utils;

using Aqua.EnumerableExtensions;

/// <summary>
/// A weak-reference cache that can be hooked-in method calls to serve cached instances
/// or transparently create the requested value if not contained in cache.
/// </summary>
public class TransparentCache<TKey, TValue>(int cleanupDelay = 2000, IEqualityComparer<TKey>? comparer = null)
    where TKey : notnull
    where TValue : class
{
    private readonly Dictionary<TKey, WeakReference> _cache = new(comparer);

    private readonly int _cleanupDelay = cleanupDelay < -1
        ? throw new ArgumentOutOfRangeException(nameof(cleanupDelay), "expected values equal or greater than -1")
        : cleanupDelay;

    private volatile bool _isCleanupScheduled;

    /// <summary>
    /// Returns the cached value if it's contained in the cache, otherwise it creates and adds the value to the cache.
    /// </summary>
    /// <remarks>
    /// This method also performes a cleanup of stale references according the cleanup-delay specified via cunstructor parameter.
    /// The cleanup task is started only if no other cleanup is pending.
    /// </remarks>
    public TValue GetOrCreate(TKey key, Func<TKey, TValue> factory)
    {
        lock (_cache)
        {
            var value = default(TValue);
            var isReferenceAlive = false;

            // probe cache
            if (_cache.TryGetValue(key, out var weakref))
            {
                value = weakref.Target as TValue;
                isReferenceAlive = weakref.IsAlive;
            }

            // create value if not found in cache
            if (!isReferenceAlive)
            {
                value = factory.CheckNotNull()(key) ?? throw new InvalidOperationException("Value factory must not return null");
                _cache[key] = new WeakReference(value);
            }

            // clean-up stale references from cache
            if (_cleanupDelay is 0)
            {
                CleanUpStaleReferences();
            }
            else if (_cleanupDelay > 0 && !_isCleanupScheduled)
            {
                _isCleanupScheduled = true;
                _ = Task.Run(async () =>
                {
                    await Task.Delay(_cleanupDelay).ConfigureAwait(false);
                    CleanUpStaleReferences();
                    _isCleanupScheduled = false;
                });
            }

            return value!;
        }
    }

    /// <summary>
    /// Removed cached items with stale references.
    /// </summary>
    protected void CleanUpStaleReferences()
    {
        lock (_cache)
        {
            _cache
                .Where(static x => !x.Value.IsAlive)
                .Select(static x => x.Key)
                .ToArray()
                .ForEach(_cache.Remove);
        }
    }
}