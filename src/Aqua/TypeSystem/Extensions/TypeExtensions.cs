// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem.Extensions
{
    using System;
    using System.Reflection;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static partial class TypeExtensions
    {
        public static bool IsAnonymousType(this Type type)
        {
            return type.Name.StartsWith("<>")
                && type.IsDefined<CompilerGeneratedAttribute>()
                || type.IsEmittedType();
        }

        public static bool IsEmittedType(this Type type)
        {
            return type.IsDefined<Emit.EmittedTypeAttribute>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsDefined<T>(this Type type) where T : Attribute
        {
            return type
#if NETSTANDARD
                .GetTypeInfo()
#endif
                .IsDefined(typeof(T));
        }
    }
}
