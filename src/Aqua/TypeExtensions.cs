// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if !NETFX_CORE && !CORECLR

namespace Aqua
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static partial class TypeExtensions
    {
        public static Type GetUnderlyingSystemType(this Type type)
        {
            return type.UnderlyingSystemType;
        }

        public static bool IsGenericType(this Type type)
        {
            return type.IsGenericType;
        }

#if NET
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

        public static bool IsEnum(this Type type)
        {
            return type.IsEnum;
        }

        public static bool IsValueType(this Type type)
        {
            return type.IsValueType;
        }

        public static bool IsSerializable(this Type type)
        {
#if SILVERLIGHT
            return false;
#else
            return type.IsSerializable;
#endif
        }

        public static Type GetBaseType(this Type type)
        {
            return type.BaseType;
        }

        public static IEnumerable<System.Reflection.MemberInfo> GetMember(this Type type, string name, Aqua.TypeSystem.MemberTypes memberType, System.Reflection.BindingFlags bindingFlags)
        {
            var t = (System.Reflection.MemberTypes)memberType;
            return type.GetMember(name, t, bindingFlags);
        }
    }
}

#endif