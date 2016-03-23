// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if WINRT

namespace Aqua
{
    using System;

    partial class TypeExtensions
    {
        internal static Type AsType(this Type type)
        {
            return type;
        }
    }
}

#endif