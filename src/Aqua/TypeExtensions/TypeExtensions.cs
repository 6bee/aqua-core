// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeExtensions;

using Aqua.TypeSystem.Emit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class TypeExtensions
{
    /// <summary>
    /// Gets the public instance fields for the given <see cref="Type"/>.
    /// </summary>
    public static IEnumerable<FieldInfo> GetDefaultFieldsForSerialization(this Type type)
        => type.GetFields(ReflectionBinding.PublicInstance);

    /// <summary>
    /// Gets the public instance fields for the give <see cref="Type"/> which are not read-only.
    /// </summary>
    public static IEnumerable<FieldInfo> GetDefaultFieldsForDeserialization(this Type type)
        => type.GetFields(ReflectionBinding.PublicInstance)
        .Where(static x => !x.IsInitOnly);

    /// <summary>
    /// Gets the public instance properties for the given <see cref="Type"/> which have a setter.
    /// </summary>
    public static IEnumerable<PropertyInfo> GetDefaultPropertiesForSerialization(this Type type)
        => type.GetProperties(ReflectionBinding.PublicInstance)
        .Where(static x => x.CanRead && x.GetIndexParameters().Length is 0);

    /// <summary>
    /// Gets the public instance properties for the given <see cref="Type"/> which have a getter.
    /// </summary>
    public static IEnumerable<PropertyInfo> GetDefaultPropertiesForDeserialization(this Type type)
        => type.GetProperties(ReflectionBinding.PublicInstance)
        .Where(static p => p.CanWrite && p.GetIndexParameters().Length is 0);

    /// <summary>
    /// Returns <see langword="true"/> if the given <see cref="Type"/> is either a reference type or a <see cref="Nullable{T}"/> value type.
    /// </summary>
    public static bool IsNullableType(this Type type)
        => !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;

    /// <summary>
    /// Tries to convert object o to targetType using implicit or explicit operator.
    /// </summary>
    internal static bool TryDynamicCast(this Type targetType, object value, out object? result)
    {
        targetType.AssertNotNull();
        value.AssertNotNull();

        var sourceType = value.GetType();
        var methodCandidates =
            sourceType.GetMethods(ReflectionBinding.PublicStatic)
            .Union(targetType.GetMethods(ReflectionBinding.PublicStatic))
            .Where(x => x.ReturnType == targetType)
            .Where(x =>
            {
                var parameters = x.GetParameters();
                return parameters.Length is 1
                    && parameters[0].ParameterType == sourceType;
            })
            .ToArray();

        var conversionMethod =
            methodCandidates.FirstOrDefault(static mi => mi.Name is "op_Implicit") ??
            methodCandidates.FirstOrDefault(static mi => mi.Name is "op_Explicit");

        if (conversionMethod is null)
        {
            result = null;
            return false;
        }

        result = conversionMethod.Invoke(null, [value]);
        return true;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the given <see cref="Type"/> is considered an anonymous type.
    /// </summary>
    public static bool IsAnonymousType(this Type type)
        => (type.CheckNotNull().Name.Contains("AnonymousType", StringComparison.Ordinal)
        && type.IsDefined<CompilerGeneratedAttribute>())
        || type.IsEmittedType();

#if NETSTANDARD2_0
    [SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Extra parameter added on purpose")]
    private static bool Contains(this string text, string value, StringComparison stringComparison) => text.Contains(value);
#endif // NETSTANDARD2_0

    /// <summary>
    /// Returns <see langword="true"/> if the given <see cref="Type"/> is a dynamically emitted type.
    /// </summary>
    public static bool IsEmittedType(this Type type) => type.IsDefined<EmittedTypeAttribute>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsDefined<T>(this Type type)
        where T : Attribute
        => type.IsDefined(typeof(T));

    /// <summary>
    /// Returns the non-nullable value type, or the type itself if <paramref name="type"/> is not of type <see cref="Nullable{T}"/>.
    /// </summary>
    public static Type AsNonNullableType(this Type type)
        => Nullable.GetUnderlyingType(type) ?? type;

    /// <summary>
    /// Returns <see langword="true"/> if the give <see cref="Type"/> is assignable to the interface type specified.
    /// </summary>
    /// <param name="type">The type to be examined.</param>
    /// <param name="interfaceType">The actualy type to be checked for.</param>
    public static bool Implements(this Type type, Type interfaceType)
    {
        type.AssertNotNull();
        interfaceType.AssertNotNull();
        return type.Implements(interfaceType, new Type[1][]);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the give <see cref="Type"/> is assignable to the interface type specified.
    /// </summary>
    /// <param name="type">The type to be examined.</param>
    /// <param name="interfaceType">The actualy type to be checked for.</param>
    /// <param name="genericTypeArguments">Out parameter with array of generic argument types, in case <paramref name="interfaceType"/> is an open generic type.</param>
    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1011:Closing square brackets should be spaced correctly", Justification = "False positive")]
    public static bool Implements(this Type type, Type interfaceType, [NotNullWhen(true)] out Type[]? genericTypeArguments)
    {
        type.AssertNotNull();
        interfaceType.AssertNotNull();
        var typeArgs = new Type[1][];
        if (type.Implements(interfaceType, typeArgs))
        {
            genericTypeArguments = typeArgs[0];
            return true;
        }

        genericTypeArguments = null;
        return false;
    }

    private static bool Implements(this Type type, Type interfaceType, Type[][] typeArgs)
    {
        var isAssignableFromSpecifiedInterface = interfaceType.IsGenericTypeDefinition
            ? IsAssignableToGenericTypeDefinition(interfaceType, typeArgs)
            : interfaceType.IsGenericType
            ? IsAssignableToGenericType(interfaceType, typeArgs)
            : interfaceType.IsAssignableFrom;

        return GetTypeHierarchy(type).Any(isAssignableFromSpecifiedInterface)
            || type.GetInterfaces().Any(isAssignableFromSpecifiedInterface);

        static IEnumerable<Type> GetTypeHierarchy(Type? t)
        {
            while (t is not null)
            {
                yield return t;
                t = t.BaseType;
            }
        }

        static Func<Type, bool> IsAssignableToGenericTypeDefinition(Type interfaceTypeInfo, Type[][] typeArgs)
        {
            return i =>
            {
                var isAssignable = false;
                if (i.IsGenericType)
                {
                    var typeDef = i.IsGenericTypeDefinition ? i : i.GetGenericTypeDefinition();
                    isAssignable = typeDef == interfaceTypeInfo;
                }

                if (isAssignable)
                {
                    typeArgs[0] = i.GenericTypeArguments;
                }

                return isAssignable;
            };
        }

        static Func<Type, bool> IsAssignableToGenericType(Type interfaceTypeInfo, Type[][] typeArgs)
        {
            var interfaceTypeDefinition = interfaceTypeInfo.GetGenericTypeDefinition();
            var interfaceGenericArguments = interfaceTypeInfo.GetGenericArguments();

            return i =>
            {
                if (i.IsGenericType && !i.IsGenericTypeDefinition)
                {
                    var typeDefinition = i.GetGenericTypeDefinition();
                    if (typeDefinition == interfaceTypeDefinition)
                    {
                        var genericArguments = i.GetGenericArguments();
                        var allArgumentsAreAssignable = Enumerable.Range(0, genericArguments.Length - 1)
                            .All(index => Implements(genericArguments[index], interfaceGenericArguments[index], typeArgs));
                        if (allArgumentsAreAssignable)
                        {
                            return true;
                        }
                    }
                }

                return false;
            };
        }
    }

    /// <summary>
    /// Returns <see langword="true"/> if the give <see cref="Type"/> is an <c>enum</c>.
    /// </summary>
    public static bool IsEnum(this Type type)
        => type.CheckNotNull().AsNonNullableType().IsEnum;

    /// <summary>
    /// Returns a formatted string for the given <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="includeNamespance"><see langword="true"/> is fullname should be included, <see langword="false"/> otherwise.</param>
    /// <param name="includeDeclaringType">Can be set <see langword="false"/> for nested types to supress name of declaring type.
    /// This has no effect for non-nested types or if <paramref name="includeNamespance"/> is <see langword="true"/>.</param>
    /// <returns>Formatted string for the given <see cref="Type"/>.</returns>
    public static string GetFriendlyName(this Type type, bool includeNamespance = true, bool includeDeclaringType = true)
        => new TypeSystem.TypeInfo(type.CheckNotNull(), false, false).GetFriendlyName(includeNamespance, includeDeclaringType);
}