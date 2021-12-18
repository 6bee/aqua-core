// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.EnumerableExtensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class EnumerableExtensions
    {
        public static IEnumerable<(T? Left, T? Right)> FullOuterJoin<T, TKey>(this IEnumerable<T> leftSet, IEnumerable<T> rightSet, Func<T, TKey> comparisonSelector, IEqualityComparer<TKey>? keyEqualityComparer = null, IEqualityComparer<(T? Left, T? Right)>? comparer = null)
            => FullOuterJoin(leftSet, rightSet, comparisonSelector, comparisonSelector, CreateTuple<T?, T?>, keyEqualityComparer, comparer);

        public static IEnumerable<TResult> FullOuterJoin<T, TKey, TResult>(this IEnumerable<T> leftSet, IEnumerable<T> rightSet, Func<T, TKey> comparisonSelector, Func<T?, T?, TResult> resultSelector, IEqualityComparer<TKey>? keyEqualityComparer = null, IEqualityComparer<TResult>? comparer = null)
            => FullOuterJoin(leftSet, rightSet, comparisonSelector, comparisonSelector, resultSelector, keyEqualityComparer, comparer);

        public static IEnumerable<(TLeft? Left, TRight? Right)> FullOuterJoin<TLeft, TRight, TKey>(this IEnumerable<TLeft> leftSet, IEnumerable<TRight> rightSet, Func<TLeft, TKey> leftKeySelector, Func<TRight, TKey> rightKeySelector, IEqualityComparer<TKey>? keyEqualityComparer = null, IEqualityComparer<(TLeft? Left, TRight? Right)>? comparer = null)
            => FullOuterJoin(leftSet, rightSet, leftKeySelector, rightKeySelector, CreateTuple<TLeft?, TRight?>, keyEqualityComparer, comparer);

        public static IEnumerable<TResult> FullOuterJoin<TLeft, TRight, TKey, TResult>(this IEnumerable<TLeft> leftSet, IEnumerable<TRight> rightSet, Func<TLeft, TKey> leftKeySelector, Func<TRight, TKey> rightKeySelector, Func<TLeft?, TRight?, TResult> resultSelector, IEqualityComparer<TKey>? keyEqualityComparer = null, IEqualityComparer<TResult>? comparer = null)
        {
            var leftOuterJoin = LeftOuterJoin(leftSet, rightSet, leftKeySelector, rightKeySelector, resultSelector, keyEqualityComparer);
            var rightOuterJoin = RightOuterJoin(leftSet, rightSet, leftKeySelector, rightKeySelector, resultSelector, keyEqualityComparer);
            return leftOuterJoin.Union(rightOuterJoin, comparer);
        }

        public static IEnumerable<(T Left, T? Right)> LeftOuterJoin<T, TKey>(this IEnumerable<T> leftSet, IEnumerable<T> rightSet, Func<T, TKey> comparisonSelector, IEqualityComparer<TKey>? keyEqualityComparer = null)
            => LeftOuterJoin(leftSet, rightSet, comparisonSelector, comparisonSelector, CreateTuple<T, T?>, keyEqualityComparer);

        public static IEnumerable<TResult> LeftOuterJoin<T, TKey, TResult>(this IEnumerable<T> leftSet, IEnumerable<T> rightSet, Func<T, TKey> comparisonSelector, Func<T, T?, TResult> resultSelector, IEqualityComparer<TKey>? keyEqualityComparer = null)
            => LeftOuterJoin(leftSet, rightSet, comparisonSelector, comparisonSelector, resultSelector, keyEqualityComparer);

        public static IEnumerable<(TLeft Left, TRight? Right)> LeftOuterJoin<TLeft, TRight, TKey>(this IEnumerable<TLeft> leftSet, IEnumerable<TRight> rightSet, Func<TLeft, TKey> leftKeySelector, Func<TRight, TKey> rightKeySelector, IEqualityComparer<TKey>? keyEqualityComparer = null)
            => LeftOuterJoin(leftSet, rightSet, leftKeySelector, rightKeySelector, CreateTuple<TLeft, TRight?>, keyEqualityComparer);

        public static IEnumerable<TResult> LeftOuterJoin<TLeft, TRight, TKey, TResult>(this IEnumerable<TLeft> leftSet, IEnumerable<TRight> rightSet, Func<TLeft, TKey> leftKeySelector, Func<TRight, TKey> rightKeySelector, Func<TLeft, TRight?, TResult> resultSelector, IEqualityComparer<TKey>? keyEqualityComparer = null)
            => leftSet
            .GroupJoin(
                rightSet,
                leftKeySelector,
                rightKeySelector,
                (k, g) => new { Key = k, Group = g },
                keyEqualityComparer)
            .SelectMany(
                x => x.Group.DefaultIfEmpty(),
                (x, y) => resultSelector(x.Key, y));

        public static IEnumerable<(T? Left, T Right)> RightOuterJoin<T, TKey>(this IEnumerable<T> leftSet, IEnumerable<T> rightSet, Func<T, TKey> comparisonSelector, IEqualityComparer<TKey>? keyEqualityComparer = null)
            => RightOuterJoin(leftSet, rightSet, comparisonSelector, comparisonSelector, CreateTuple<T?, T>, keyEqualityComparer);

        public static IEnumerable<TResult> RightOuterJoin<T, TKey, TResult>(this IEnumerable<T> leftSet, IEnumerable<T> rightSet, Func<T, TKey> comparisonSelector, Func<T?, T, TResult> resultSelector, IEqualityComparer<TKey>? keyEqualityComparer = null)
            => RightOuterJoin(leftSet, rightSet, comparisonSelector, comparisonSelector, resultSelector, keyEqualityComparer);

        public static IEnumerable<(TLeft? Left, TRight Right)> RightOuterJoin<TLeft, TRight, TKey>(this IEnumerable<TLeft> leftSet, IEnumerable<TRight> rightSet, Func<TLeft, TKey> leftKeySelector, Func<TRight, TKey> rightKeySelector, IEqualityComparer<TKey>? keyEqualityComparer = null)
            => RightOuterJoin(leftSet, rightSet, leftKeySelector, rightKeySelector, CreateTuple<TLeft?, TRight>, keyEqualityComparer);

        public static IEnumerable<TResult> RightOuterJoin<TLeft, TRight, TKey, TResult>(this IEnumerable<TLeft> leftSet, IEnumerable<TRight> rightSet, Func<TLeft, TKey> leftKeySelector, Func<TRight, TKey> rightKeySelector, Func<TLeft?, TRight, TResult> resultSelector, IEqualityComparer<TKey>? keyEqualityComparer = null)
            => rightSet
            .GroupJoin(
                leftSet,
                rightKeySelector,
                leftKeySelector,
                (k, g) => new { Key = k, Group = g },
                keyEqualityComparer)
            .SelectMany(
                x => x.Group.DefaultIfEmpty(),
                (x, y) => resultSelector(y, x.Key));

        /// <summary>
        /// Compares two collections for equality considering the same number of equal elements regardles of the elements sort order.
        /// </summary>
        /// <returns><see langword="true"/> if the collections are equal, <see langword="false"/> otherwise.</returns>
        public static bool CollectionEquals<T>(this IEnumerable<T>? collection1, IEnumerable<T>? collection2) => CollectionEquals(collection1, collection2, null);

        /// <summary>
        /// Compares two collections for equality considering the same number of equal elements regardles of the elements sort order.
        /// </summary>
        /// <returns><see langword="true"/> if the collections are equal, <see langword="false"/> otherwise.</returns>
        public static bool CollectionEquals<T>(this IEnumerable<T>? collection1, IEnumerable<T>? collection2, IEqualityComparer<T>? comparer)
        {
            if (collection1 is null && collection2 is null)
            {
                return true;
            }

            if (collection1 is null)
            {
                return !collection2.Any();
            }

            if (collection2 is null)
            {
                return !collection1.Any();
            }

#nullable disable
            var counters = new Dictionary<T, int>(comparer);
#nullable restore
            var nullCounter = 0;

            foreach (T s in collection1)
            {
                if (s is null)
                {
                    nullCounter++;
                }
                else if (counters.ContainsKey(s))
                {
                    counters[s]++;
                }
                else
                {
                    counters.Add(s, 1);
                }
            }

            foreach (T s in collection2)
            {
                if (s is null)
                {
                    nullCounter--;
                }
                else if (counters.ContainsKey(s))
                {
                    counters[s]--;
                }
                else
                {
                    return false;
                }
            }

            return nullCounter == 0 && counters.Values.All(c => c == 0);
        }

        /// <summary>
        /// Computes a collection's hash code based on the elements contained. The hash code is not affected by the sort order.
        /// </summary>
        /// <returns>The collection's hash code.</returns>
        public static int GetCollectionHashCode<T>(this IEnumerable<T>? collection) => GetCollectionHashCode(collection, null);

        /// <summary>
        /// Computes a collection's hash code based on the elements contained. The hash code is not affected by the sort order.
        /// </summary>
        /// <returns>The collection's hash code.</returns>
        public static int GetCollectionHashCode<T>(this IEnumerable<T>? collection, IEqualityComparer<T>? comparer)
        {
            comparer ??= EqualityComparer<T>.Default;

            unchecked
            {
                var hashCode = 0;

                if (collection is not null)
                {
                    foreach (var item in collection)
                    {
                        hashCode ^= item is null ? -1 : comparer.GetHashCode(item);
                    }
                }

                return hashCode;
            }
        }

        /// <summary>
        /// Returns <see langword="true"/> if the object is of type <see cref="IEnumerable"/> but not <see cref="string"/>.
        /// </summary>
#pragma warning disable S3874 // SonarCSharp_S3874: "out" and "ref" parameters should not be used
        public static bool IsCollection(this object? obj, [NotNullWhen(true)] out IEnumerable? enumerable)
        {
            if (obj is IEnumerable x && obj is not string)
            {
                enumerable = x;
                return true;
            }

            enumerable = null;
            return false;
        }
#pragma warning restore S3874 // SonarCSharp_S3874: "out" and "ref" parameters should not be used

        public static IEnumerable CastCollectionToArrayOfType(this IEnumerable items, Type elementType)
        {
            var castedItems = MethodInfos.Enumerable.Cast.MakeGenericMethod(elementType).Invoke(null, new[] { items });
            var array = MethodInfos.Enumerable.ToArray.MakeGenericMethod(elementType).Invoke(null, new[] { castedItems });
            return (IEnumerable)array!;
        }

        public static IEnumerable CastCollectionToListOfType(this IEnumerable items, Type elementType)
        {
            var castedItems = MethodInfos.Enumerable.Cast.MakeGenericMethod(elementType).Invoke(null, new[] { items });
            var list = MethodInfos.Enumerable.ToList.MakeGenericMethod(elementType).Invoke(null, new[] { castedItems });
            return (IEnumerable)list!;
        }

        public static IEnumerable<T> AsEmptyIfNull<T>(this IEnumerable<T>? source) => source ?? Enumerable.Empty<T>();

        public static IEnumerable<T>? AsNullIfEmpty<T>(this IEnumerable<T>? source) => source?.Any() is true ? source : null;

        [return: NotNullIfNotNull("source")]
        public static string? StringJoin<T>(this IEnumerable<T>? source, string separator) => source is null ? default : string.Join(separator, source);

        [DebuggerStepThrough]
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }

        [DebuggerStepThrough]
        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            var index = 0;
            foreach (var item in source)
            {
                action(item, index++);
            }
        }

        [DebuggerStepThrough]
        public static void ForEach<T, TResult>(this IEnumerable<T> source, Func<T, TResult> func)
        {
            foreach (var item in source)
            {
                _ = func(item);
            }
        }

        [DebuggerStepThrough]
        public static void ForEach<T, TResult>(this IEnumerable<T> source, Func<T, int, TResult> func)
        {
            var index = 0;
            foreach (var item in source)
            {
                _ = func(item, index++);
            }
        }

        public static bool Any([NotNullWhen(true)] this IEnumerable? source) => source?.GetEnumerator().MoveNext() is true;

        public static bool Any([NotNullWhen(true)] this IEnumerable? source, Func<object, bool> predicate)
        {
            if (source is null)
            {
                return false;
            }

#pragma warning disable S3267 // False positive: there's no linq extension method to simplify loop with
            foreach (var item in source)
#pragma warning restore S3267 // False positive: there's no linq extension method to simplify loop with
            {
                if (predicate(item))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool All(this IEnumerable? source, Func<object, bool> predicate)
        {
            if (source is null)
            {
                return true;
            }

            foreach (var item in source)
            {
                if (!predicate(item))
                {
                    return false;
                }
            }

            return true;
        }

#if NETSTANDARD2_0
        internal static HashSet<T> ToHashSet<T>(this IEnumerable<T> source) => new HashSet<T>(source);
#endif // NETSTANDARD2_0

        [SuppressMessage("Major Code Smell", "S1172:Unused method parameters should be removed", Justification = "Indeed the parameters are being used")]
        private static (TLeft Left, TRight Right) CreateTuple<TLeft, TRight>(TLeft left, TRight right) => (left, right);
    }
}