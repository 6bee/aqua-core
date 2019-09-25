// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class EnumerableExtensions
    {
        private static class Tuple
        {
            public static Tuple<TLeft, TRight> Create<TLeft, TRight>(TLeft left, TRight right)
                => new Tuple<TLeft, TRight>(left, right);
        }

        [DebuggerDisplay("{Left}|{Right}")]
        public sealed class Tuple<TLeft, TRight>
        {
            public Tuple(TLeft left, TRight right)
            {
                Left = left;
                Right = right;
            }

            public TLeft Left { get; }

            public TRight Right { get; }

            private bool Equals(Tuple<TLeft, TRight> other)
                => Equals(Left, other.Left)
                && Equals(Right, other.Right);

            public override bool Equals(object obj)
            {
                if (obj is null)
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                return obj is Tuple<TLeft, TRight> && Equals((Tuple<TLeft, TRight>)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Left?.GetHashCode() ?? 0) * 397) ^ (Right?.GetHashCode() ?? 0);
                }
            }
        }

        public static IEnumerable<Tuple<T, T>> FullOuterJoin<T, TKey>(this IEnumerable<T> leftSet, IEnumerable<T> rightSet, Func<T, TKey> comparisonSelector, IEqualityComparer<TKey> keyEqualityComparer = null, IEqualityComparer<Tuple<T, T>> comparer = null)
            => FullOuterJoin(leftSet, rightSet, comparisonSelector, comparisonSelector, Tuple.Create, keyEqualityComparer, comparer);

        public static IEnumerable<TResult> FullOuterJoin<T, TKey, TResult>(this IEnumerable<T> leftSet, IEnumerable<T> rightSet, Func<T, TKey> comparisonSelector, Func<T, T, TResult> resultSelector, IEqualityComparer<TKey> keyEqualityComparer = null, IEqualityComparer<TResult> comparer = null)
            => FullOuterJoin(leftSet, rightSet, comparisonSelector, comparisonSelector, resultSelector, keyEqualityComparer, comparer);

        public static IEnumerable<Tuple<TLeft, TRight>> FullOuterJoin<TLeft, TRight, TKey>(this IEnumerable<TLeft> leftSet, IEnumerable<TRight> rightSet, Func<TLeft, TKey> leftKeySelector, Func<TRight, TKey> rightKeySelector, IEqualityComparer<TKey> keyEqualityComparer = null, IEqualityComparer<Tuple<TLeft, TRight>> comparer = null)
            => FullOuterJoin(leftSet, rightSet, leftKeySelector, rightKeySelector, Tuple.Create, keyEqualityComparer, comparer);

        public static IEnumerable<TResult> FullOuterJoin<TLeft, TRight, TKey, TResult>(this IEnumerable<TLeft> leftSet, IEnumerable<TRight> rightSet, Func<TLeft, TKey> leftKeySelector, Func<TRight, TKey> rightKeySelector, Func<TLeft, TRight, TResult> resultSelector, IEqualityComparer<TKey> keyEqualityComparer = null, IEqualityComparer<TResult> comparer = null)
        {
            var leftOuterJoin = LeftOuterJoin(leftSet, rightSet, leftKeySelector, rightKeySelector, resultSelector, keyEqualityComparer);
            var rightOuterJoin = RightOuterJoin(leftSet, rightSet, leftKeySelector, rightKeySelector, resultSelector, keyEqualityComparer);
            return leftOuterJoin.Union(rightOuterJoin, comparer);
        }

        public static IEnumerable<Tuple<T, T>> LeftOuterJoin<T, TKey>(this IEnumerable<T> leftSet, IEnumerable<T> rightSet, Func<T, TKey> comparisonSelector, IEqualityComparer<TKey> keyEqualityComparer = null)
            => LeftOuterJoin(leftSet, rightSet, comparisonSelector, comparisonSelector, Tuple.Create, keyEqualityComparer);

        public static IEnumerable<TResult> LeftOuterJoin<T, TKey, TResult>(this IEnumerable<T> leftSet, IEnumerable<T> rightSet, Func<T, TKey> comparisonSelector, Func<T, T, TResult> resultSelector, IEqualityComparer<TKey> keyEqualityComparer = null)
            => LeftOuterJoin(leftSet, rightSet, comparisonSelector, comparisonSelector, resultSelector, keyEqualityComparer);

        public static IEnumerable<Tuple<TLeft, TRight>> LeftOuterJoin<TLeft, TRight, TKey>(this IEnumerable<TLeft> leftSet, IEnumerable<TRight> rightSet, Func<TLeft, TKey> leftKeySelector, Func<TRight, TKey> rightKeySelector, IEqualityComparer<TKey> keyEqualityComparer = null)
            => LeftOuterJoin(leftSet, rightSet, leftKeySelector, rightKeySelector, Tuple.Create, keyEqualityComparer);

        public static IEnumerable<TResult> LeftOuterJoin<TLeft, TRight, TKey, TResult>(this IEnumerable<TLeft> leftSet, IEnumerable<TRight> rightSet, Func<TLeft, TKey> leftKeySelector, Func<TRight, TKey> rightKeySelector, Func<TLeft, TRight, TResult> resultSelector, IEqualityComparer<TKey> keyEqualityComparer = null)
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

        public static IEnumerable<Tuple<T, T>> RightOuterJoin<T, TKey>(this IEnumerable<T> leftSet, IEnumerable<T> rightSet, Func<T, TKey> comparisonSelector, IEqualityComparer<TKey> keyEqualityComparer = null)
            => RightOuterJoin(leftSet, rightSet, comparisonSelector, comparisonSelector, Tuple.Create, keyEqualityComparer);

        public static IEnumerable<TResult> RightOuterJoin<T, TKey, TResult>(this IEnumerable<T> leftSet, IEnumerable<T> rightSet, Func<T, TKey> comparisonSelector, Func<T, T, TResult> resultSelector, IEqualityComparer<TKey> keyEqualityComparer = null)
            => RightOuterJoin(leftSet, rightSet, comparisonSelector, comparisonSelector, resultSelector, keyEqualityComparer);

        public static IEnumerable<Tuple<TLeft, TRight>> RightOuterJoin<TLeft, TRight, TKey>(this IEnumerable<TLeft> leftSet, IEnumerable<TRight> rightSet, Func<TLeft, TKey> leftKeySelector, Func<TRight, TKey> rightKeySelector, IEqualityComparer<TKey> keyEqualityComparer = null)
            => RightOuterJoin(leftSet, rightSet, leftKeySelector, rightKeySelector, Tuple.Create, keyEqualityComparer);

        public static IEnumerable<TResult> RightOuterJoin<TLeft, TRight, TKey, TResult>(this IEnumerable<TLeft> leftSet, IEnumerable<TRight> rightSet, Func<TLeft, TKey> leftKeySelector, Func<TRight, TKey> rightKeySelector, Func<TLeft, TRight, TResult> resultSelector, IEqualityComparer<TKey> keyEqualityComparer = null)
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

        public static bool CollectionEquals<T>(this IEnumerable<T> collection1, IEnumerable<T> collection2) => CollectionEquals(collection1, collection2, null);

        public static bool CollectionEquals<T>(this IEnumerable<T> collection1, IEnumerable<T> collection2, IEqualityComparer<T> comparer)
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

            var counters = new Dictionary<T, int>(comparer);
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

        public static int GetCollectionHashCode<T>(this IEnumerable<T> collection) => GetCollectionHashCode(collection, null);

        public static int GetCollectionHashCode<T>(this IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            if (comparer is null)
            {
                comparer = EqualityComparer<T>.Default;
            }

            unchecked
            {
                var hashCode = 0;

                if (!(collection is null))
                {
                    foreach (var item in collection)
                    {
                        hashCode ^= item is null ? -1 : comparer.GetHashCode(item);
                    }
                }

                return hashCode;
            }
        }
    }
}
