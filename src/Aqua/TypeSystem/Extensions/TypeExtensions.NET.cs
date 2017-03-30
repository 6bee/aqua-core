// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if NET

namespace Aqua.TypeSystem.Extensions
{
    using System;
    using System.Reflection;
    using System.Collections.Generic;

    partial class TypeExtensions
    {
        public static Type GetUnderlyingSystemType(this Type type)
        {
            return type.UnderlyingSystemType;
        }

        public static bool IsGenericType(this Type type)
        {
            return type.IsGenericType;
        }

        public static bool IsGenericTypeDefinition(this Type type)
        {
            return type.IsGenericTypeDefinition;
        }

        public static bool IsEnum(this Type type)
        {
            return type.IsEnum ||
                (type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                && type.GetGenericArguments()[0].IsEnum());
        }

        public static bool IsValueType(this Type type)
        {
            return type.IsValueType;
        }

        public static bool IsSerializable(this Type type)
        {
            return type.IsSerializable;
        }

        public static Type GetBaseType(this Type type)
        {
            return type.BaseType;
        }

        public static IEnumerable<MemberInfo> GetMember(this Type type, string name, Aqua.TypeSystem.MemberTypes memberType, BindingFlags bindingFlags)
        {
            var t = (MemberTypes)memberType;
            return type.GetMember(name, t, bindingFlags);
        }
    }
}

#endif