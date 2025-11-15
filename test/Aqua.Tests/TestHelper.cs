// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests;

using Aqua.TypeSystem;
using Shouldly;
using System.Collections;
using System.Globalization;

public static class TestHelper
{
    public static bool TestIs<T>(this object test)
        where T : class
        => test.GetType() == typeof(T);

    public static bool ValueIs(this object value, Type type)
        => value?.GetType() == type;

    public static bool Is<T>(this Type type)
    {
        if (type == typeof(T) ||
            typeof(ICollection<T>).IsAssignableFrom(type))
        {
            return true;
        }

        if (typeof(T).IsValueType)
        {
            var nullableType = typeof(Nullable<>).MakeGenericType(typeof(T));
            if (type == nullableType ||
                typeof(ICollection<>).MakeGenericType(nullableType).IsAssignableFrom(type))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsNotPublic(this Type type)
        => type.IsNotPublic
        || TypeHelper.GetElementType(type).IsNotPublic;

    public static bool IsEnum(this Type type)
    {
        if (type.IsEnum)
        {
            return true;
        }

        var elementType = TypeHelper.GetElementType(type);
        return (type.IsCollection() && elementType.IsEnum)
            || (type.IsGenericType && typeof(Nullable<>) == type.GetGenericTypeDefinition() && type.GetGenericArguments()[0].IsEnum)
            || (type.IsCollection() && elementType.IsGenericType && typeof(Nullable<>) == elementType.GetGenericTypeDefinition() && elementType.GetGenericArguments()[0].IsEnum);
    }

    public static bool IsCollection(this Type type)
        => typeof(IEnumerable).IsAssignableFrom(type)
        && type != typeof(string);

    public static IDisposable CreateContext(this CultureInfo culture) => new CultureContext(culture);

    private sealed class CultureContext : IDisposable
    {
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly CultureInfo _culture;

        public CultureContext(CultureInfo culture)
        {
            _semaphore.Wait();
            _culture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = culture;
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _culture;
            _semaphore.Release();
        }
    }

    public static void SequenceShouldBeEqual<T>(this IEnumerable<T> result, IEnumerable<T> expected)
        => result.SequenceEqual(expected).ShouldBeTrue();

    public static void SequenceShouldBeEqual<T>(this IEnumerable<T> result, IEnumerable<T> expected, IEqualityComparer<T> comparer)
        => result.SequenceEqual(expected, comparer).ShouldBeTrue();

    public static void SequenceShouldBeEqual<T>(this IEnumerable<T> result, IEnumerable<T> expected, Func<T, T, bool> comparer)
        => result.SequenceShouldBeEqual(expected, new SimpleEqualityComparer<T>(comparer));

    public static bool SequenceEqual<T>(this IEnumerable<T> result, IEnumerable<T> expected, Func<T, T, bool> comparer)
        => result.SequenceEqual(expected, new SimpleEqualityComparer<T>(comparer));

    private sealed class SimpleEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _compare;

        public SimpleEqualityComparer(Func<T, T, bool> compare)
        {
            _compare = compare ?? throw new ArgumentNullException(nameof(compare));
        }

        public bool Equals(T x, T y) => _compare(x, y);

        public int GetHashCode(T obj) => 0;
    }
}