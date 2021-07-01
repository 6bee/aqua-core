// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeExtensions
{
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
        private const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;

        /// <summary>
        /// Gets the public instance fields for the given <see cref="Type"/>.
        /// </summary>
        public static IEnumerable<FieldInfo> GetDefaultFieldsForSerialization(this Type type)
            => type.GetFields(PublicInstance);

        /// <summary>
        /// Gets the public instance fields for the give <see cref="Type"/> which are not read-only.
        /// </summary>
        public static IEnumerable<FieldInfo> GetDefaultFieldsForDeserialization(this Type type)
            => type.GetFields(PublicInstance)
            .Where(x => !x.IsInitOnly);

        /// <summary>
        /// Gets the public instance properties for the given <see cref="Type"/> which have a setter.
        /// </summary>
        public static IEnumerable<PropertyInfo> GetDefaultPropertiesForSerialization(this Type type)
            => type.GetProperties(PublicInstance)
            .Where(x => x.CanRead && x.GetIndexParameters().Length == 0);

        /// <summary>
        /// Gets the public instance properties for the given <see cref="Type"/> which have a getter.
        /// </summary>
        public static IEnumerable<PropertyInfo> GetDefaultPropertiesForDeserialization(this Type type)
            => type.GetProperties(PublicInstance)
            .Where(p => p.CanWrite && p.GetIndexParameters().Length == 0);

        /// <summary>
        /// Returns <see langword="true"/> if the given <see cref="Type"/> is either a reference type or a <see cref="Nullable{T}"/> value type.
        /// </summary>
        public static bool IsNullableType(this Type type)
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

            const BindingFlags PublicStatic = BindingFlags.Static | BindingFlags.Public;

            var sourceType = value.GetType();
            var methodCandidates =
                sourceType.GetMethods(PublicStatic)
                .Union(targetType.GetMethods(PublicStatic))
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

        /// <summary>
        /// Returns <see langword="true"/> if the given <see cref="Type"/> is considered an anonymous type.
        /// </summary>
        public static bool IsAnonymousType(this Type type)
            => (type.CheckNotNull(nameof(type)).Name.Contains("AnonymousType", StringComparison.Ordinal)
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
        {
            var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            return isNullable ? type.GetGenericArguments()[0] : type;
        }

        /// <summary>
        /// Returns <see langword="true"/> if the give <see cref="Type"/> is assignable to the interface type specified.
        /// </summary>
        /// <param name="type">The type to be examined.</param>
        /// <param name="interfaceType">The actualy type to be checked for.</param>
        public static bool Implements(this Type type, Type interfaceType)
            => type.CheckNotNull(nameof(type)).Implements(interfaceType, new Type[1][]);

        /// <summary>
        /// Returns <see langword="true"/> if the give <see cref="Type"/> is assignable to the interface type specified.
        /// </summary>
        /// <param name="type">The type to be examined.</param>
        /// <param name="interfaceType">The actualy type to be checked for.</param>
        /// <param name="genericTypeArguments">Out parameter with array of generic argument types, in case <paramref name="interfaceType"/> is an open generic type.</param>
#pragma warning disable SA1011 // Closing square brackets should be spaced correctly
#pragma warning disable S3874 // SonarCSharp_S3874: "out" and "ref" parameters should not be used
        public static bool Implements(this Type type, Type interfaceType, [NotNullWhen(true)] out Type[]? genericTypeArguments)
#pragma warning restore S3874 // SonarCSharp_S3874: "out" and "ref" parameters should not be used
#pragma warning restore SA1011 // Closing square brackets should be spaced correctly
        {
            var typeArgs = new Type[1][];
            if (type.CheckNotNull(nameof(type)).Implements(interfaceType, typeArgs))
            {
                genericTypeArguments = typeArgs[0];
                return true;
            }

            genericTypeArguments = null;
            return false;
        }

        private static bool Implements(this Type type, Type interfaceType, Type[][] typeArgs)
        {
            var isAssignableFromSpecifiedInterface = interfaceType.CheckNotNull(nameof(interfaceType)).IsGenericTypeDefinition
                ? IsAssignableToGenericTypeDefinition(interfaceType, typeArgs)
                : interfaceType.IsGenericType
                ? IsAssignableToGenericType(interfaceType, typeArgs)
                : interfaceType.IsAssignableFrom;

            return isAssignableFromSpecifiedInterface(type)
                || type.GetInterfaces().Any(isAssignableFromSpecifiedInterface);
        }

        private static Func<Type, bool> IsAssignableToGenericTypeDefinition(Type interfaceTypeInfo, Type[][] typeArgs)
        {
            var genericArgumentsCount = interfaceTypeInfo.GetGenericArguments().Length;

            return i =>
            {
                var genericArguments = i.GenericTypeArguments;
                var isAssignable = i.IsGenericType && genericArguments.Length == genericArgumentsCount;
                if (isAssignable)
                {
                    try
                    {
                        isAssignable = interfaceTypeInfo.MakeGenericType(genericArguments).IsAssignableFrom(i);
                    }
                    catch (ArgumentException)
                    {
                        // justification:
                        // https://stackoverflow.com/questions/4864496/checking-if-an-object-meets-a-generic-parameter-constraint/4864565#4864565
                        isAssignable = false;
                    }
                }

                if (isAssignable)
                {
                    typeArgs[0] = genericArguments;
                }

                return isAssignable;
            };
        }

        private static Func<Type, bool> IsAssignableToGenericType(Type interfaceTypeInfo, Type[][] typeArgs)
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

        /// <summary>
        /// Returns <see langword="true"/> if the give <see cref="Type"/> is an <c>enum</c>.
        /// </summary>
        public static bool IsEnum(this Type type)
            => type.CheckNotNull(nameof(type)).AsNonNullableType().IsEnum;
    }
}
