// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests
{
    using Aqua.TypeSystem;
    using Shouldly;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class Helper
    {
        public static bool TestIs<T>(this object test)
            where T : class
            => test.GetType() == typeof(T);

        public static bool ValueIs(this object value, Type type)
            => value?.GetType() == type;

        public static bool Is<T>(this Type type)
            where T : struct
            => type == typeof(T)
            || type == typeof(T?)
            || typeof(ICollection<T>).IsAssignableFrom(type)
            || typeof(ICollection<T?>).IsAssignableFrom(type);

        public static bool IsEnum(this Type type)
            => type.IsEnum
            || (type.IsCollection() && TypeHelper.GetElementType(type).IsEnum)
            || (type.IsGenericType && typeof(Nullable<>) == type.GetGenericTypeDefinition() && type.GetGenericArguments()[0].IsEnum)
            || (type.IsCollection() && TypeHelper.GetElementType(type).IsGenericType && typeof(Nullable<>) == TypeHelper.GetElementType(type).GetGenericTypeDefinition() && TypeHelper.GetElementType(type).GetGenericArguments()[0].IsEnum);

        public static bool IsCollection(this Type type)
            => typeof(IEnumerable).IsAssignableFrom(type)
            && type != typeof(string);

        public static IDisposable CreateContext(this CultureInfo culture) => new CultureContext(culture);

        private sealed class CultureContext : IDisposable
        {
            private readonly CultureInfo _culture;

            public CultureContext(CultureInfo culture)
            {
                _culture = CultureInfo.CurrentCulture;
                CultureInfo.CurrentCulture = culture;
            }

            public void Dispose()
            {
                CultureInfo.CurrentCulture = _culture;
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
}