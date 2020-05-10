// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    public static class TypeHelper
    {
        private const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
        private const BindingFlags Any = PublicInstance | BindingFlags.NonPublic | BindingFlags.Static;

        /// <summary>
        /// Returns the element type of the collection type specified, except for typeof(string).
        /// </summary>
        /// <param name="type">The collection type.</param>
        /// <returns>Null if string or not collection type, the element type otherwise.</returns>
        public static Type? GetElementType(Type? type)
        {
            if (type is null)
            {
                return null;
            }

            if (type.IsArray)
            {
                return type.GetElementType();
            }

            var enumerableType = FindIEnumerable(type);
            if (enumerableType is null)
            {
                return type;
            }

            return enumerableType.GetGenericArguments().First();
        }

        private static Type? FindIEnumerable(Type? type)
        {
            if (type is null || type == typeof(string))
            {
                return null;
            }

            if (type.IsArray)
            {
                return typeof(IEnumerable<>).MakeGenericType(type.GetElementType());
            }

            if (type.IsGenericType)
            {
                foreach (var arg in type.GetGenericArguments())
                {
                    var enumerableType = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (enumerableType.IsAssignableFrom(type))
                    {
                        return enumerableType;
                    }
                }
            }

            var interfaces = type.GetInterfaces();
            if (interfaces != null && interfaces.Any())
            {
                foreach (var interfaceType in interfaces)
                {
                    var enumerableType = FindIEnumerable(interfaceType);
                    if (enumerableType != null)
                    {
                        return enumerableType;
                    }
                }
            }

            var baseType = type.BaseType;
            if (baseType != null && baseType != typeof(object))
            {
                return FindIEnumerable(baseType);
            }

            return null;
        }

        [return: NotNullIfNotNull("typeInfo")]
        public static Type? ResolveType(this TypeInfo? typeInfo, ITypeResolver typeResolver) => typeResolver.ResolveType(typeInfo);

        public static System.Reflection.MemberInfo? ResolveMemberInfo(this MemberInfo? memberInfo, ITypeResolver typeResolver)
            => memberInfo?.MemberType switch
            {
                null => null,
                MemberTypes.Field => ((FieldInfo)memberInfo).ResolveField(typeResolver),
                MemberTypes.Constructor => ((ConstructorInfo)memberInfo).ResolveConstructor(typeResolver),
                MemberTypes.Property => ((PropertyInfo)memberInfo).ResolveProperty(typeResolver),
                MemberTypes.Method => ((MethodInfo)memberInfo).ResolveMethod(typeResolver),
                _ => throw new ArgumentException($"Unknown member type: {memberInfo.MemberType}"),
            };

        public static System.Reflection.ConstructorInfo? ResolveConstructor(this ConstructorInfo? constructorInfo, ITypeResolver typeResolver)
            => CreateConstructorResolver(constructorInfo, typeResolver)(Any);

        public static System.Reflection.ConstructorInfo? ResolveConstructor(this ConstructorInfo? constructorInfo, ITypeResolver typeResolver, BindingFlags bindingFlags)
            => CreateConstructorResolver(constructorInfo, typeResolver)(bindingFlags);

        public static System.Reflection.FieldInfo? ResolveField(this FieldInfo? fieldInfo, ITypeResolver typeResolver)
        {
            var fieldResolver = CreateFieldResolver(fieldInfo, typeResolver);
            return fieldResolver(Any | BindingFlags.DeclaredOnly)
                ?? fieldResolver(Any);
        }

        public static System.Reflection.FieldInfo? ResolveField(this FieldInfo? fieldInfo, ITypeResolver typeResolver, BindingFlags bindingAttr)
            => CreateFieldResolver(fieldInfo, typeResolver)(bindingAttr);

        public static System.Reflection.MethodInfo? ResolveMethod(this MethodInfo? methodInfo, ITypeResolver typeResolver)
        {
            var methodResolver = CreateMethodResolver(methodInfo, typeResolver);
            return methodResolver(Any | BindingFlags.DeclaredOnly)
                ?? methodResolver(Any);
        }

        public static System.Reflection.MethodInfo? ResolveMethod(this MethodInfo? methodInfo, ITypeResolver typeResolver, BindingFlags bindingflags)
            => CreateMethodResolver(methodInfo, typeResolver)(bindingflags);

        public static System.Reflection.PropertyInfo? ResolveProperty(this PropertyInfo? propertyInfo, ITypeResolver typeResolver)
        {
            var propertyResolver = CreatePropertyResolver(propertyInfo, typeResolver);
            return propertyResolver(Any | BindingFlags.DeclaredOnly)
                ?? propertyResolver(Any);
        }

        public static System.Reflection.PropertyInfo? ResolveProperty(this PropertyInfo? propertyInfo, ITypeResolver typeResolver, BindingFlags bindingAttr)
            => CreatePropertyResolver(propertyInfo, typeResolver)(bindingAttr);

        private static Func<BindingFlags, System.Reflection.ConstructorInfo?> CreateConstructorResolver(ConstructorInfo? constructorInfo, ITypeResolver typeResolver)
        {
            if (constructorInfo is null)
            {
                return _ => null;
            }

            Exception CreateException(string reason, Exception? innerException = null)
                => new TypeResolverException($"Failed to resolve constructor '{constructorInfo}' since {reason}.");

            var declaringType = constructorInfo.ResolveDeclaringType(typeResolver);

            var parameterTypes = constructorInfo.ParameterTypes?
                .Select(typeInfo =>
                {
                    try
                    {
                        return typeResolver.ResolveType(typeInfo);
                    }
                    catch (Exception ex)
                    {
                        throw CreateException($"parameter type '{typeInfo}' could not be reconstructed", ex);
                    }
                })
                .ToArray()
                ?? Array.Empty<Type>();

            var constructorName = constructorInfo.Name;
            var isStatic = constructorInfo.IsStatic ?? false;
            System.Reflection.ConstructorInfo? Filter(System.Reflection.ConstructorInfo[] candidates)
            {
                var matches = candidates
                    .Where(m => string.Equals(m.Name, constructorName, StringComparison.Ordinal))
                    .Where(m => m.IsStatic == isStatic)
                    .Where(m => m.GetParameters().Length == parameterTypes.Length)
                    .Where(m =>
                    {
                        var paramTypes = m.GetParameters();
                        for (int i = 0; i < paramTypes.Length; i++)
                        {
                            if (paramTypes[i].ParameterType != parameterTypes![i])
                            {
                                return false;
                            }
                        }

                        return true;
                    })
                    .ToArray();

                if (matches.Length == 0)
                {
                    return null;
                }

                if (matches.Length == 1)
                {
                    return matches[0];
                }

                throw CreateException($"{matches.Length} constructors found matching criteria");
            }

            return bindingflags => Filter(declaringType.GetConstructors(bindingflags));
        }

        private static Func<BindingFlags, System.Reflection.FieldInfo?> CreateFieldResolver(FieldInfo? fieldInfo, ITypeResolver typeResolver)
        {
            if (fieldInfo is null)
            {
                return _ => null;
            }

            var declaringType = fieldInfo.ResolveDeclaringType(typeResolver);
            var fieldName = fieldInfo.Name;
            var isStatic = fieldInfo.IsStatic ?? false;
            return bindingFlags => declaringType
                .GetField(fieldName, bindingFlags)
                .If(x => x.IsStatic == isStatic);
        }

        private static Func<BindingFlags, System.Reflection.MethodInfo?> CreateMethodResolver(MethodInfo? methodInfo, ITypeResolver typeResolver)
        {
            if (methodInfo is null)
            {
                return _ => null;
            }

            Exception CreateException(string reason, Exception? innerException = null)
                => new TypeResolverException($"Failed to resolve method '{methodInfo}' since {reason}.");

            var declaringType = methodInfo.ResolveDeclaringType(typeResolver);
            if (declaringType is null)
            {
                throw CreateException("no declaring type specified");
            }

            var isGenericMethod = methodInfo.IsGenericMethod;
            var genericArguments = methodInfo.GenericArgumentTypes?
                .Select(typeInfo =>
                {
                    try
                    {
                        var genericArgumentType = typeResolver.ResolveType(typeInfo);
                        return genericArgumentType;
                    }
                    catch (Exception ex)
                    {
                        throw CreateException($"generic argument type '{typeInfo}' could not be reconstructed", ex);
                    }
                })
                .ToArray();

            if (isGenericMethod && genericArguments is null)
            {
                throw CreateException("open generic method type is not supported");
            }

            var parameterTypes = methodInfo.ParameterTypes?
                .Select(typeInfo =>
                {
                    try
                    {
                        var parameterType = typeResolver.ResolveType(typeInfo);
                        return parameterType;
                    }
                    catch (Exception ex)
                    {
                        throw CreateException($"parameter type '{typeInfo}' could not be reconstructed", ex);
                    }
                })
                .ToArray()
                ?? Array.Empty<Type>();

            Type? returnType;
            try
            {
                returnType = typeResolver.ResolveType(methodInfo.ReturnType);
            }
            catch (Exception ex)
            {
                throw CreateException($"return type '{methodInfo.ReturnType}' could not be reconstructed", ex);
            }

            int CountDeclarationDepth(System.Reflection.TypeInfo type, System.Reflection.TypeInfo methodDeclaringType, int i)
                => type == methodDeclaringType
                ? i
                : CountDeclarationDepth(type.BaseType.GetTypeInfo(), methodDeclaringType, i + 1);

            var isStatic = methodInfo.IsStatic ?? false;
            var methodName = methodInfo.Name;
            System.Reflection.MethodInfo? Filter(System.Reflection.MethodInfo[] candidates)
            {
                var matches = candidates
                    .Where(m => string.Equals(m.Name, methodName, StringComparison.Ordinal))
                    .Where(m => m.IsStatic == isStatic)
                    .Where(m => m.GetParameters().Length == parameterTypes.Length)
                    .Where(m => m.IsGenericMethod == isGenericMethod)
                    .Where(m => !m.IsGenericMethod || m.GetGenericArguments().Length == genericArguments!.Length)
                    .Select(m => m.IsGenericMethod ? m.MakeGenericMethod(genericArguments) : m)
                    .Where(m =>
                    {
                        var paramTypes = m.GetParameters();
                        for (int i = 0; i < paramTypes.Length; i++)
                        {
                            if (paramTypes[i].ParameterType != parameterTypes[i])
                            {
                                return false;
                            }
                        }

                        return true;
                    })
                    .Where(m => returnType is null || m.ReturnType == returnType)
                    .OrderBy(m => CountDeclarationDepth(declaringType.GetTypeInfo(), m.DeclaringType.GetTypeInfo(), 0))
                    .ToArray();

                if (matches.Length == 0)
                {
                    return null;
                }

                if (matches.Length == 1)
                {
                    return matches[0];
                }

                throw CreateException($"{matches.Length} methods found matching criteria");
            }

            return bindingflags => Filter(declaringType.GetMethods(bindingflags));
        }

        private static Func<BindingFlags, System.Reflection.PropertyInfo?> CreatePropertyResolver(PropertyInfo? propertyInfo, ITypeResolver typeResolver)
        {
            if (propertyInfo is null)
            {
                return _ => null;
            }

            var declaringType = propertyInfo.ResolveDeclaringType(typeResolver);
            var propertyName = propertyInfo.Name;
            var isStatic = propertyInfo.IsStatic ?? false;
            return bindingFlags => declaringType
                .GetProperty(propertyName, bindingFlags)
                .If(x => (x.GetGetMethod(true) ?? x.GetSetMethod(true)).IsStatic == isStatic);
        }

        /// <summary>
        /// Returns null if the condition is not met, the actual value otherwise.
        /// </summary>
        private static T? If<T>(this T t, Func<T, bool> predicate)
            where T : class
            => t != null && predicate(t) ? t : default;

        private static Type ResolveDeclaringType(this MemberInfo memberInfo, ITypeResolver typeResolver)
        {
            Exception CreateException(string what, Exception? innerException = null)
                => new TypeResolverException($"{what} for {memberInfo.MemberType.ToString().ToLower()} {memberInfo}", innerException);

            if (memberInfo.DeclaringType is null)
            {
                throw CreateException("No declaring type specified");
            }

            try
            {
                return typeResolver.ResolveType(memberInfo.DeclaringType);
            }
            catch (Exception ex)
            {
                throw CreateException($"Declaring type '{memberInfo.DeclaringType}' could not be reconstructed", ex);
            }
        }
    }
}
