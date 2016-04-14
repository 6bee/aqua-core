// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if NET35 || SILVERLIGHT

namespace Aqua.TypeSystem.Extensions
{
    using System;
    using System.Reflection;

    partial class TypeExtensions
    {
        public static Type GetTypeInfo(this Type type)
        {
            return type;
        }

        public static Type AsType(this Type type)
        {
            return type;
        }

        public static T GetCustomAttribute<T>(this MemberInfo type) where T : Attribute
        {
            return type.GetCustomAttributes(typeof(T), true) as T;
        }

        public static void SetValue(this PropertyInfo property, object obj, object value)
        {
            property.SetValue(obj, value, null);
        }

        public static object GetValue(this PropertyInfo property, object obj)
        {
            return property.GetValue(obj, null);
        }
    }
}

#endif