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

        /// <summary>
        /// Tries to convert object o to targetType using implicit or explicit operator.
        /// </summary>
        internal static bool TryDynamicCast(this Type targetType, object value, out object? result)
        {
            if (targetType is null)
            {
                throw new ArgumentNullException(nameof(targetType));
            }

            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var sourceType = value.GetType();
            var methodCandidates =
                sourceType.GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Union(targetType.GetMethods(BindingFlags.Static | BindingFlags.Public))
                .Where(x => x.ReturnType == targetType)
                .Where(x =>
                {
                    var parameters = x.GetParameters();
                    return parameters.Length == 1
                        && parameters[0].ParameterType == sourceType;
                })
                .ToArray();

            var conversionMethod =
                methodCandidates.FirstOrDefault(mi => mi.Name == "op_Implicit") ??
                methodCandidates.FirstOrDefault(mi => mi.Name == "op_Explicit");

            if (conversionMethod is null)
            {
                result = null;
                return false;
            }

            result = conversionMethod.Invoke(null, new[] { value });
            return true;
        }
    }
}
