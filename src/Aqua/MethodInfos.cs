// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua;

using Aqua.TypeExtensions;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

internal static class MethodInfos
{
#pragma warning disable S2094 // Classes should not be empty
    /// <summary>
    /// Type definition used in generic type filters.
    /// </summary>
    private sealed class TElement
    {
    }

    /// <summary>
    /// Type definition used in generic type filters.
    /// </summary>
    private sealed class TResult
    {
    }

    /// <summary>
    /// Type definition used in generic type filters.
    /// </summary>
    private sealed class TSource
    {
    }
#pragma warning restore S2094 // Classes should not be empty

    internal static class Enumerable
    {
        internal static readonly MethodInfo Cast = typeof(System.Linq.Enumerable).GetMethodEx(
            nameof(System.Linq.Enumerable.Cast),
            [typeof(TResult)],
            typeof(IEnumerable));

        internal static readonly MethodInfo ToArray = typeof(System.Linq.Enumerable).GetMethodEx(
            nameof(System.Linq.Enumerable.ToArray),
            [typeof(TSource)],
            typeof(IEnumerable<TSource>));

        internal static readonly MethodInfo ToList = typeof(System.Linq.Enumerable).GetMethodEx(
            nameof(System.Linq.Enumerable.ToList),
            [typeof(TSource)],
            typeof(IEnumerable<TSource>));
    }

    internal static class Queryable
    {
        internal static readonly MethodInfo AsQueryable = typeof(System.Linq.Queryable).GetMethodEx(
            nameof(System.Linq.Queryable.AsQueryable),
            [typeof(TElement)],
            typeof(IEnumerable<TElement>));
    }
}