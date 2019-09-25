// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests
{
    using System;

    public static class Helper
    {
        public static bool TestIs<T>(this object test) where T : class
            => test.GetType() == typeof(T);

        public static bool Is<T>(this Type type) where T : struct
            => type == typeof(T)
            || type == typeof(T?)
            || type == typeof(T[])
            || type == typeof(T?[]);
    }
}