// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua
{
    using Aqua.TypeExtensions;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal static class MethodInfos
    {
        /// <summary>
        /// Type definition used in generic type filters.
        /// </summary>
        [SuppressMessage("Major Bug", "S3453:Classes should not have only \"private\" constructors", Justification = "For reflection only")]
        private sealed class TElement
        {
            private TElement()
            {
            }
        }

        /// <summary>
        /// Type definition used in generic type filters.
        /// </summary>
        [SuppressMessage("Major Bug", "S3453:Classes should not have only \"private\" constructors", Justification = "For reflection only")]
        private sealed class TDelegate
        {
            private TDelegate()
            {
            }
        }

        /// <summary>
        /// Type definition used in generic type filters.
        /// </summary>
        [SuppressMessage("Major Bug", "S3453:Classes should not have only \"private\" constructors", Justification = "For reflection only")]
        private sealed class TKey
        {
            private TKey()
            {
            }
        }

        /// <summary>
        /// Type definition used in generic type filters.
        /// </summary>
        [SuppressMessage("Major Bug", "S3453:Classes should not have only \"private\" constructors", Justification = "For reflection only")]
        private sealed class TResult
        {
            private TResult()
            {
            }
        }

        /// <summary>
        /// Type definition used in generic type filters.
        /// </summary>
        [SuppressMessage("Major Bug", "S3453:Classes should not have only \"private\" constructors", Justification = "For reflection only")]
        private sealed class TSource
        {
            private TSource()
            {
            }
        }

        internal static class Enumerable
        {
            internal static readonly MethodInfo Cast = typeof(System.Linq.Enumerable).GetMethodEx(
                nameof(System.Linq.Enumerable.Cast),
                new[] { typeof(TResult) },
                typeof(IEnumerable));

            internal static readonly MethodInfo OfType = typeof(System.Linq.Enumerable).GetMethodEx(
                nameof(System.Linq.Enumerable.OfType),
                new[] { typeof(TResult) },
                typeof(IEnumerable));

            internal static readonly MethodInfo ToArray = typeof(System.Linq.Enumerable).GetMethodEx(
                nameof(System.Linq.Enumerable.ToArray),
                new[] { typeof(TSource) },
                typeof(IEnumerable<TSource>));

            internal static readonly MethodInfo ToList = typeof(System.Linq.Enumerable).GetMethodEx(
                nameof(System.Linq.Enumerable.ToList),
                new[] { typeof(TSource) },
                typeof(IEnumerable<TSource>));

            internal static readonly MethodInfo Contains = typeof(System.Linq.Enumerable).GetMethodEx(
                nameof(System.Linq.Enumerable.Contains),
                new[] { typeof(TSource) },
                typeof(IEnumerable<TSource>),
                typeof(TSource));

            internal static readonly MethodInfo Single = typeof(System.Linq.Enumerable).GetMethodEx(
                nameof(System.Linq.Enumerable.Single),
                new[] { typeof(TSource) },
                typeof(IEnumerable<TSource>));

            internal static readonly MethodInfo SingleOrDefault = typeof(System.Linq.Enumerable).GetMethodEx(
                nameof(System.Linq.Enumerable.SingleOrDefault),
                new[] { typeof(TSource) },
                typeof(IEnumerable<TSource>));
        }

        internal static class Expression
        {
            internal static readonly MethodInfo Lambda = typeof(System.Linq.Expressions.Expression).GetMethodEx(
                nameof(System.Linq.Expressions.Expression.Lambda),
                new[] { typeof(TDelegate) },
                typeof(Expression),
                typeof(ParameterExpression[]));
        }

        internal static class Queryable
        {
            internal static readonly MethodInfo AsQueryable = typeof(System.Linq.Queryable).GetMethodEx(
                nameof(System.Linq.Queryable.AsQueryable),
                new[] { typeof(TElement) },
                typeof(IEnumerable<TElement>));

            internal static readonly MethodInfo OrderBy = typeof(System.Linq.Queryable).GetMethodEx(
                nameof(System.Linq.Queryable.OrderBy),
                new[] { typeof(TSource), typeof(TKey) },
                typeof(IQueryable<TSource>),
                typeof(Expression<Func<TSource, TKey>>));

            internal static readonly MethodInfo OrderByDescending = typeof(System.Linq.Queryable).GetMethodEx(
                nameof(System.Linq.Queryable.OrderByDescending),
                new[] { typeof(TSource), typeof(TKey) },
                typeof(IQueryable<TSource>),
                typeof(Expression<Func<TSource, TKey>>));

            internal static readonly MethodInfo ThenBy = typeof(System.Linq.Queryable).GetMethodEx(
                nameof(System.Linq.Queryable.ThenBy),
                new[] { typeof(TSource), typeof(TKey) },
                typeof(IOrderedQueryable<TSource>),
                typeof(Expression<Func<TSource, TKey>>));

            internal static readonly MethodInfo ThenByDescending = typeof(System.Linq.Queryable).GetMethodEx(
                nameof(System.Linq.Queryable.ThenByDescending),
                new[] { typeof(TSource), typeof(TKey) },
                typeof(IOrderedQueryable<TSource>),
                typeof(Expression<Func<TSource, TKey>>));

            internal static readonly MethodInfo Select = typeof(System.Linq.Queryable).GetMethodEx(
                nameof(System.Linq.Queryable.Select),
                new[] { typeof(TSource), typeof(TResult) },
                typeof(IOrderedQueryable<TSource>),
                typeof(Expression<Func<TSource, TResult>>));
        }

        internal static class String
        {
            internal static readonly MethodInfo StartsWith = typeof(string).GetMethodEx(
                nameof(string.StartsWith),
                typeof(string));

            internal static readonly MethodInfo EndsWith = typeof(string).GetMethodEx(
                nameof(string.EndsWith),
                typeof(string));

            internal static readonly MethodInfo Contains = typeof(string).GetMethodEx(
                nameof(string.Contains),
                typeof(string));
        }
    }
}