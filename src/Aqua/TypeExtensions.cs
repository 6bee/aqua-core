// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua
{
    using System;
    using System.Reflection;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static partial class TypeExtensions
    {

#if NET || NET35 || CORECLR

        public static bool IsAnonymousType(this Type type)
        {
            return type.Name.StartsWith("<>")
                && type.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false).Any()
                || type.IsEmittedType();
        }

        public static bool IsEmittedType(this Type type)
        {
            return type.GetCustomAttributes(typeof(Aqua.TypeSystem.Emit.EmittedTypeAttribute), false).Any();
        }

#else

        public static bool IsAnonymousType(this Type type)
        {
            return type.Name.StartsWith("<>")
                && type.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false).Any();
        }

#endif
    }
}