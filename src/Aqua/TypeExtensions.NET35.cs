// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if NET35 || SILVERLIGHT

namespace Aqua
{
    using System;
    using System.Reflection;

    partial class TypeExtensions
    {
        internal static Type GetTypeInfo(this Type type)
        {
            return type;
        }

        internal static Type AsType(this Type type)
        {
            return type;
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