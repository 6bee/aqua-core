// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Dynamic;

using System.Reflection;

internal static class UnmappedAttributeHelper
{
    internal static bool HasNoUnmappedAnnotation(PropertyInfo property) => property.GetCustomAttribute<UnmappedAttribute>() is null;

    internal static bool HasNoUnmappedAnnotation(FieldInfo field) => field.GetCustomAttribute<UnmappedAttribute>() is null;
}
