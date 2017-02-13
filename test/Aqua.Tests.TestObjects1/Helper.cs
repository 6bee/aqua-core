// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.TestObjects1
{
    using System;
    using TestObjects;

    public static class Helper
    {
        public static Type GetAnonymousType0<T1, T2>()
        {
            return new { P1 = default(T1), P2 = default(T2) }.GetType();
        }

        public static Type GetAnonymousType1()
        {
            return new { Foo = 1 }.GetType();
        }

        public static Type GetCustomType0()
        {
            return typeof(CustomType0);
        }

        public static Type GetCustomType1()
        {
            return typeof(CustomType1);
        }

        public static Type GetCustomType2()
        {
            return typeof(CustomType2);
        }
    }
}

namespace Aqua.Tests.TestObjects
{
    internal class CustomType0
    {
        public int Foo { get; set; }
    }

    internal class CustomType1
    {
        public int Foo { get; set; }
    }

    internal class CustomType2
    {
        public int Foo { get; set; }
    }
}