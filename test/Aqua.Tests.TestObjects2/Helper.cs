// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TestObjects2
{
    using Aqua.Tests.TestObjects;
    using System;

    public static class Helper
    {
        public static Type GetAnonymousType0<T1, T2>()
        {
            return new { P1 = default(T1), P2 = default(T2) }.GetType();
        }

        public static Type GetAnonymousType1()
        {
            return new { Bar = 1 }.GetType();
        }

        public static Type GetAnonymousTypeYX()
        {
            return new { Y = 2, X = 1.0 }.GetType();
        }

        public static Type GetCustomType0()
        {
            return typeof(CustomType0);
        }

        public static Type GetCustomType1()
        {
            return typeof(CustomType1);
        }
    }
}
