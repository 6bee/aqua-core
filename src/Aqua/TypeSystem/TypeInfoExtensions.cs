// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using Aqua.EnumerableExtensions;
    using System.ComponentModel;
    using System.Linq;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class TypeInfoExtensions
    {
        public static string PrintFriendlyName(this TypeInfo typeInfo, bool includeNamespance = true)
        {
            var genericArgumentsString = typeInfo.CheckNotNull(nameof(typeInfo)).GetGenericArgumentsString();
            var typeName = includeNamespance
                ? typeInfo.FullName
                : typeInfo.NameWithoutNameSpace;
            if (typeInfo.IsArray)
            {
                typeName = typeName.Substring(0, typeName.Length - 2);
            }

            return $"{typeName}{genericArgumentsString}{(typeInfo.IsArray ? "[]" : null)}";
        }

        private static string? GetGenericArgumentsString(this TypeInfo typeInfo)
        {
            var genericArguments = typeInfo.GenericArguments;
            var genericArgumentsString = typeInfo.IsGenericType && (genericArguments?.Any() ?? false)
                ? $"[{genericArguments.StringJoin(",")}]"
                : null;
            return genericArgumentsString;
        }
    }
}
