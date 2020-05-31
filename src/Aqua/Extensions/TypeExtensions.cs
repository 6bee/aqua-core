// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class TypeExtensions
    {
        private const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;

        public static IEnumerable<FieldInfo> GetDefaultFieldsForSerialization(this Type type)
            => type.GetFields(PublicInstance);

        public static IEnumerable<FieldInfo> GetDefaultFieldsForDeserialization(this Type type)
            => type.GetFields(PublicInstance)
            .Where(x => !x.IsInitOnly);

        public static IEnumerable<PropertyInfo> GetDefaultPropertiesForSerialization(this Type type)
            => type.GetProperties(PublicInstance)
            .Where(x => x.CanRead && x.GetIndexParameters().Length == 0);

        public static IEnumerable<PropertyInfo> GetDefaultPropertiesForDeserialization(this Type type)
            => type.GetProperties(PublicInstance)
            .Where(p => p.CanWrite && p.GetIndexParameters().Length == 0);

        public static bool IsNullable(this Type type)
            => type.IsClass
            || (type.IsGenericType && typeof(Nullable<>) == type.GetGenericTypeDefinition());
    }
}
