// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem
{
    using Aqua.EnumerableExtensions;
    using Aqua.TypeExtensions;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    public static class TypeHelper
    {
        /// <summary>
        /// Returns the element type of the collection type specified, except for typeof(string).
        /// </summary>
        /// <param name="type">The collection type.</param>
        /// <returns>Collection element type if <paramref name="type"/> is a collection type. If <paramref name="type"/> is a non-collection or string type, <paramref name="type"/> is returned.</returns>
        [return: NotNullIfNotNull("type")]
        public static Type? GetElementType(Type? type)
        {
            if (type is null)
            {
                return null;
            }

            if (type.IsArray)
            {
                return type.GetElementType()!;
            }

            var enumerableType = FindIEnumerable(type);
            if (enumerableType is not null)
            {
                return enumerableType.GetGenericArguments()[0];
            }

            return type;
        }

        private static Type? FindIEnumerable(Type? type)
        {
            if (type is null || type == typeof(string))
            {
                return null;
            }

            if (type.IsArray)
            {
                return typeof(IEnumerable<>).MakeGenericType(type.GetElementType()!);
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
            if (interfaces is not null && interfaces.Any())
            {
                foreach (var interfaceType in interfaces)
                {
                    var enumerableType = FindIEnumerable(interfaceType);
                    if (enumerableType is not null)
                    {
                        return enumerableType;
                    }
                }
            }

            var baseType = type.BaseType;
            if (baseType is not null && baseType != typeof(object))
            {
                return FindIEnumerable(baseType);
            }

            return null;
        }

        [return: NotNullIfNotNull("type")]
        public static Type? ResolveType(this TypeInfo? type, ITypeResolver typeResolver)
            => typeResolver.CheckNotNull().ResolveType(type);

        [return: NotNullIfNotNull("member")]
        public static System.Reflection.MemberInfo? ResolveMemberInfo(this MemberInfo? member, ITypeResolver typeResolver)
            => member?.MemberType switch
            {
                null => null,
                MemberTypes.Field => ((FieldInfo)member).ResolveField(typeResolver),
                MemberTypes.Constructor => ((ConstructorInfo)member).ResolveConstructor(typeResolver),
                MemberTypes.Property => ((PropertyInfo)member).ResolveProperty(typeResolver),
                MemberTypes.Method => ((MethodInfo)member).ResolveMethod(typeResolver),
                _ => throw new ArgumentException($"Unknown member type: {member.MemberType}"),
            };

        public static System.Reflection.ConstructorInfo? ResolveConstructor(this ConstructorInfo? constructor, ITypeResolver typeResolver)
            => constructor is null
            ? null
            : CreateConstructorResolver(constructor, typeResolver.CheckNotNull())(ReflectionBinding.Any);

        public static System.Reflection.ConstructorInfo? ResolveConstructor(this ConstructorInfo? constructor, ITypeResolver typeResolver, BindingFlags bindingFlags)
            => constructor is null
            ? null
            : CreateConstructorResolver(constructor, typeResolver.CheckNotNull())(bindingFlags);

        public static System.Reflection.FieldInfo? ResolveField(this FieldInfo? field, ITypeResolver typeResolver)
        {
            if (field is null)
            {
                return null;
            }

            var fieldResolver = CreateFieldResolver(field, typeResolver);
            return fieldResolver(ReflectionBinding.Any | BindingFlags.DeclaredOnly)
                ?? fieldResolver(ReflectionBinding.Any);
        }

        public static System.Reflection.FieldInfo? ResolveField(this FieldInfo? field, ITypeResolver typeResolver, BindingFlags bindingAttr)
            => field is null
            ? null
            : CreateFieldResolver(field, typeResolver)(bindingAttr);

        public static System.Reflection.MethodInfo? ResolveMethod(this MethodInfo? method, ITypeResolver typeResolver)
        {
            if (method is null)
            {
                return null;
            }

            var methodResolver = CreateMethodResolver(method, typeResolver.CheckNotNull());
            return methodResolver(ReflectionBinding.Any | BindingFlags.DeclaredOnly)
                ?? methodResolver(ReflectionBinding.Any);
        }

        public static System.Reflection.MethodInfo? ResolveMethod(this MethodInfo? method, ITypeResolver typeResolver, BindingFlags bindingflags)
            => method is null
            ? null
            : CreateMethodResolver(method, typeResolver.CheckNotNull())(bindingflags);

        public static System.Reflection.PropertyInfo? ResolveProperty(this PropertyInfo? property, ITypeResolver typeResolver)
        {
            if (property is null)
            {
                return null;
            }

            var propertyResolver = CreatePropertyResolver(property, typeResolver);
            return propertyResolver(ReflectionBinding.Any | BindingFlags.DeclaredOnly)
                ?? propertyResolver(ReflectionBinding.Any);
        }

        public static System.Reflection.PropertyInfo? ResolveProperty(this PropertyInfo? property, ITypeResolver typeResolver, BindingFlags bindingAttr)
            => property is null
            ? null
            : CreatePropertyResolver(property, typeResolver)(bindingAttr);

        private static Func<BindingFlags, System.Reflection.ConstructorInfo?> CreateConstructorResolver(ConstructorInfo constructor, ITypeResolver typeResolver)
        {
            Exception CreateException(string reason, Exception? innerException = null)
                => new TypeResolverException($"Failed to resolve constructor '{constructor}' since {reason}.", innerException);

            var declaringType = constructor.ResolveDeclaringType(typeResolver);

            Type[] parameterTypes = constructor
                .ParameterTypes
                .AsEmptyIfNull()
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
                .ToArray();

            var constructorName = constructor.Name;
            var isStatic = constructor.IsStatic ?? false;
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

        private static Func<BindingFlags, System.Reflection.FieldInfo?> CreateFieldResolver(FieldInfo field, ITypeResolver typeResolver)
        {
            var declaringType = field.ResolveDeclaringType(typeResolver);
            var fieldName = field.Name ?? string.Empty;
            var isStatic = field.IsStatic ?? false;
            return bindingFlags => declaringType
                .GetField(fieldName, bindingFlags)
                .If(x => x!.IsStatic == isStatic);
        }

        private static Func<BindingFlags, System.Reflection.MethodInfo?> CreateMethodResolver(MethodInfo method, ITypeResolver typeResolver)
        {
            Exception CreateException(string reason, Exception? innerException = null)
                => new TypeResolverException($"Failed to resolve method '{method}' since {reason}.", innerException);

            var declaringType = method.ResolveDeclaringType(typeResolver);
            if (declaringType is null)
            {
                throw CreateException("no declaring type specified");
            }

            var isGenericMethod = method.IsGenericMethod;
            var genericArguments = method.GenericArgumentTypes?
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

            Type[] parameterTypes = method
                .ParameterTypes
                .AsEmptyIfNull()
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
                .ToArray();

            Type? returnType;
            try
            {
                returnType = typeResolver.ResolveType(method.ReturnType);
            }
            catch (Exception ex)
            {
                throw CreateException($"return type '{method.ReturnType}' could not be reconstructed", ex);
            }

            static int CountDeclarationDepth(System.Reflection.TypeInfo type, System.Reflection.TypeInfo? methodDeclaringType, int i)
                => type == methodDeclaringType || type?.BaseType is null
                ? i
                : CountDeclarationDepth(type.BaseType.GetTypeInfo(), methodDeclaringType, i + 1);

            var isStatic = method.IsStatic ?? false;
            var methodName = method.Name;
            System.Reflection.MethodInfo? Filter(System.Reflection.MethodInfo[] candidates)
            {
                var matches = candidates
                    .Where(m => string.Equals(m.Name, methodName, StringComparison.Ordinal))
                    .Where(m => m.IsStatic == isStatic)
                    .Where(m => m.GetParameters().Length == parameterTypes.Length)
                    .Where(m => m.IsGenericMethod == isGenericMethod)
                    .Where(m => !m.IsGenericMethod || m.GetGenericArguments().Length == genericArguments!.Length)
                    .Select(m => m.IsGenericMethod && genericArguments is not null ? m.MakeGenericMethod(genericArguments) : m)
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
                    .OrderBy(m => CountDeclarationDepth(declaringType.GetTypeInfo(), m.DeclaringType?.GetTypeInfo(), 0))
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

        private static Func<BindingFlags, System.Reflection.PropertyInfo?> CreatePropertyResolver(PropertyInfo property, ITypeResolver typeResolver)
        {
            var declaringType = property.ResolveDeclaringType(typeResolver);
            var propertyName = property.Name ?? string.Empty;
            var isStatic = property.IsStatic ?? false;
            return bindingFlags => declaringType
                .GetProperty(propertyName, bindingFlags)
                .If(x => (x!.GetGetMethod(true) ?? x!.GetSetMethod(true))!.IsStatic == isStatic);
        }

        /// <summary>
        /// Returns <see langword="null"/> if the condition is not met, otherwise the actual value is returned.
        /// </summary>
        private static T? If<T>(this T? t, Func<T, bool> predicate)
            where T : class
            => t is not null && predicate(t) ? t : default;

        private static Type ResolveDeclaringType(this MemberInfo member, ITypeResolver typeResolver)
        {
            Exception CreateException(string what, Exception? innerException = null)
                => new TypeResolverException($"{what} for {member.MemberType.ToString().ToLower(CultureInfo.CurrentCulture)} {member}", innerException);

            if (member.DeclaringType is null)
            {
                throw CreateException("No declaring type specified");
            }

            try
            {
                return typeResolver.ResolveType(member.DeclaringType)
                    ?? throw CreateException("Type resolver returned null");
            }
            catch (Exception ex)
            {
                throw CreateException($"Declaring type '{member.DeclaringType}' could not be reconstructed", ex);
            }
        }
    }
}
