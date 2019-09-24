// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if !NETSTANDARD1_X

namespace Aqua.TypeSystem.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    partial class TypeExtensions
    {
        public static Type GetUnderlyingSystemType(this Type type) => type.UnderlyingSystemType;

        public static bool IsGenericType(this Type type) => type.IsGenericType;

        public static bool IsGenericTypeDefinition(this Type type) => type.IsGenericTypeDefinition;

        public static bool IsEnum(this Type type)
            => type.IsEnum ||
                (type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                && type.GetGenericArguments()[0].IsEnum());

        public static bool IsValueType(this Type type) => type.IsValueType;

        public static bool IsSerializable(this Type type) => type.IsSerializable;

        public static Type GetBaseType(this Type type) => type.BaseType;

        public static IEnumerable<MemberInfo> GetMember(this Type type, string name, Aqua.TypeSystem.MemberTypes memberType, BindingFlags bindingFlags)
            => type.GetMember(name, (MemberTypes)memberType, bindingFlags);
    }
}

#endif