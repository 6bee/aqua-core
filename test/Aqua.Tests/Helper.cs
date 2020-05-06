// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests
{
    using System;
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
            || type == typeof(T[])
            || type == typeof(T?[]);

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