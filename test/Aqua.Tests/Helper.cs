// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests
{
    using Aqua.TypeSystem;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class Helper
    {
        public static bool TestIs<T>(this object test)
            where T : class
            => test.GetType() == typeof(T);

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
    }
}